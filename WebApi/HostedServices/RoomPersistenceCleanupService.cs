using StackExchange.Redis;
using WebApi.Application.Repositories;
using WebApi.Application.Services;
using WebApi.Core.Entities;

namespace WebApi.HostedServices;

public class RoomPersistenceCleanupService : BackgroundService
{
    private static readonly TimeSpan ScanInterval = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan PresenceGracePeriod = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan CandidateKeyLifetime = TimeSpan.FromHours(1);
    private static readonly TimeSpan OrphanFileGracePeriod = TimeSpan.FromHours(1);
    private static readonly TimeSpan CleanupLeaseLifetime = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan RoomInactivityPeriod = TimeSpan.FromHours(1);
    private const string CleanupLeaseKey = "shared-watch:cleanup:rooms:lease";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubConnectionRegistry _connections;
    private readonly IUploadedVideoStorage _videoStorage;
    private readonly IDatabase _redis;
    private readonly ILogger<RoomPersistenceCleanupService> _logger;
    private readonly IRoomActivityTracker _activityTracker;

    public RoomPersistenceCleanupService(
        IServiceScopeFactory scopeFactory,
        IHubConnectionRegistry connections,
        IUploadedVideoStorage videoStorage,
        IConnectionMultiplexer redis,
        ILogger<RoomPersistenceCleanupService> logger,
        IRoomActivityTracker activityTracker)
    {
        _scopeFactory = scopeFactory;
        _connections = connections;
        _videoStorage = videoStorage;
        _redis = redis.GetDatabase();
        _logger = logger;
        _activityTracker = activityTracker;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(ScanInterval, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunSweepAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Room persistence cleanup sweep failed.");
            }

            await Task.Delay(ScanInterval, stoppingToken);
        }
    }

    private async Task RunSweepAsync(CancellationToken cancellationToken)
    {
        var leaseToken = Guid.NewGuid().ToString("N");
        if (!await _redis.StringSetAsync(CleanupLeaseKey, leaseToken, CleanupLeaseLifetime, When.NotExists))
        {
            return;
        }

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var roomRepository = scope.ServiceProvider.GetRequiredService<IRoomRepository>();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var roomLifecycle = scope.ServiceProvider.GetRequiredService<IRoomLifecycleService>();
            var rooms = await roomRepository.GetRoomsForCleanupAsync(cancellationToken);

            foreach (var room in rooms)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var roomHash = room.Hash;
                if (string.IsNullOrWhiteSpace(roomHash))
                {
                    continue;
                }

                if (HasLiveConnection(roomHash, room.Users.Select(user => user.Username)))
                {
                    if (await _activityTracker.IsInactiveAsync(roomHash, RoomInactivityPeriod))
                    {
                        var deletedRoom = await roomRepository.DeleteRoomAsync(roomHash, cancellationToken);
                        if (deletedRoom is not null)
                        {
                            _logger.LogInformation(
                                "Removed inactive room {RoomHash} after {InactivityPeriod}.",
                                roomHash,
                                RoomInactivityPeriod);
                        }

                        await _activityTracker.RemoveAsync(roomHash);
                    }

                    await _redis.KeyDeleteAsync(GetCandidateKey(roomHash));
                    continue;
                }

                var candidateKey = GetCandidateKey(roomHash);
                var candidateAgeMs = (long)await _redis.ScriptEvaluateAsync("""
                    local t = redis.call('TIME')
                    local now = tonumber(t[1]) * 1000 + math.floor(tonumber(t[2]) / 1000)
                    local started = redis.call('GET', KEYS[1])
                    if not started then
                        redis.call('SET', KEYS[1], now, 'PX', ARGV[1])
                        return 0
                    end
                    return math.max(0, now - tonumber(started))
                    """, [candidateKey], [(long)CandidateKeyLifetime.TotalMilliseconds]);
                if (candidateAgeMs < PresenceGracePeriod.TotalMilliseconds)
                {
                    continue;
                }

                var currentRoom = await roomRepository.GetRoomAsync(roomHash, cancellationToken);
                if (currentRoom is null)
                {
                    await _redis.KeyDeleteAsync(candidateKey);
                    continue;
                }

                var aConnectionReturned = false;
                foreach (var user in currentRoom.Users.ToList())
                {
                    if (_connections.GetConnectionIdsByUserId(roomHash, user.Username).Count > 0)
                    {
                        aConnectionReturned = true;
                        break;
                    }

                    await userRepository.DeleteUserByUsernameAsync(roomHash, user.Username, cancellationToken);
                }

                if (aConnectionReturned)
                {
                    await _redis.KeyDeleteAsync(candidateKey);
                    continue;
                }

                await roomLifecycle.DeleteIfEmptyAsync(roomHash, cancellationToken);
                await _redis.KeyDeleteAsync(candidateKey);
            }

            await ReclaimOrphanFilesAsync(rooms, cancellationToken);
        }
        finally
        {
            await _redis.ScriptEvaluateAsync(
                "if redis.call('GET', KEYS[1]) == ARGV[1] then return redis.call('DEL', KEYS[1]) else return 0 end",
                [CleanupLeaseKey], [leaseToken]);
        }
    }

    private async Task ReclaimOrphanFilesAsync(
        IReadOnlyCollection<Room> rooms,
        CancellationToken cancellationToken)
    {
        var referencedFiles = rooms
            .SelectMany(room => room.PlaylistVideos)
            .Where(video => video.Url?.StartsWith("/api/VideoFiles/", StringComparison.Ordinal) == true)
            .Select(video => Path.GetFileName(video.Url) ?? string.Empty)
            .Where(fileName => fileName.Length > 0)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var oldestSafeTimestamp = DateTime.UtcNow - OrphanFileGracePeriod;

        foreach (var file in _videoStorage.ListFiles())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (file.LastModifiedUtc > oldestSafeTimestamp || referencedFiles.Contains(file.FileName))
            {
                continue;
            }

            try
            {
                await _videoStorage.DeleteAsync(file.FileName, cancellationToken);
                _logger.LogInformation("Removed unreferenced uploaded video file {FileName}.", file.FileName);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.LogWarning(exception, "Could not remove orphaned uploaded video file {FileName}.", file.FileName);
            }
        }
    }

    private bool HasLiveConnection(string roomHash, IEnumerable<string> usernames) =>
        usernames.Any(username => _connections.GetConnectionIdsByUserId(roomHash, username).Count > 0);

    private static RedisKey GetCandidateKey(string roomHash) => $"shared-watch:cleanup:rooms:candidate:{roomHash}";
}



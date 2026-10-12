using System.Text.Json;
using StackExchange.Redis;
using WebApi.Application.Services;
using WebApi.Core.Entities;
using WebApi.Core.Entities.In_memory;
namespace WebApi.Infrastructure.Redis;
public sealed class RedisVideoPlayerStateService : IVideoPlayerStateService
{
    private readonly IDatabase _redis;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    public RedisVideoPlayerStateService(IConnectionMultiplexer redis) => _redis = redis.GetDatabase();
    public double GetCurrentTime(string roomHash)
    {
        var key = Key(roomHash);
        return GetEffectiveTime(key, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    }

    public bool SetCurrentTime(string roomHash, double currentTime)
    {
        var key = Key(roomHash);
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        _redis.HashSet(key, new HashEntry[]
        {
            new("currentTime", Math.Max(0, currentTime)),
            new("serverTimestampUnixMs", now)
        });
        _redis.HashIncrement(key, "revision", 1);
        return true;
    }

    public bool GetIsPlaying(string roomHash) => ReadBoolean(_redis.HashGet(Key(roomHash), "isPlaying"));

    public bool SetIsPlaying(string roomHash, bool isPlaying)
    {
        var key = Key(roomHash);
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var currentTime = GetEffectiveTime(key, now);
        _redis.HashSet(key, new HashEntry[]
        {
            new("currentTime", currentTime),
            new("isPlaying", isPlaying),
            new("serverTimestampUnixMs", now)
        });
        _redis.HashIncrement(key, "revision", 1);
        return true;
    }

    public PlaylistVideo? GetCurrentVideo(string roomHash) => ReadVideo(_redis.HashGet(Key(roomHash), "currentVideo"));
    public void SetCurrentVideo(string roomHash, PlaylistVideo? video)
    {
        var key = Key(roomHash);
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (video is null)
        {
            _redis.HashDelete(key, "currentVideo");
            _redis.HashSet(key, new HashEntry[]
            {
                new("currentTime", 0),
                new("isPlaying", false),
                new("isCurrentVideoRemoved", false),
                new("serverTimestampUnixMs", now)
            });
            _redis.HashIncrement(key, "revision", 1);
            return;
        }

        _redis.HashSet(key, new HashEntry[]
        {
            new("currentVideo", JsonSerializer.Serialize(video, JsonOptions)),
            new("currentTime", 0),
            new("isPlaying", false),
            new("isCurrentVideoRemoved", false),
            new("serverTimestampUnixMs", now)
        });
        _redis.HashIncrement(key, "revision", 1);
    }

    public void SetIsCurrentVideoRemoved(string roomHash, bool isRemoved)
    {
        var key = Key(roomHash);
        _redis.HashSet(key, new HashEntry[]
        {
            new("isCurrentVideoRemoved", isRemoved),
            new("serverTimestampUnixMs", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
        });
        _redis.HashIncrement(key, "revision", 1);
    }

    public bool GetIsCurrentVideoRemoved(string roomHash) => ReadBoolean(_redis.HashGet(Key(roomHash), "isCurrentVideoRemoved"));

    public VideoPlayer? GetVideoPlayer(string roomHash)
    {
        var key = Key(roomHash);
        if (!_redis.KeyExists(key))
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return new VideoPlayer
        {
            CurrentTime = GetEffectiveTime(key, now),
            IsPlaying = ReadBoolean(_redis.HashGet(key, "isPlaying")),
            PlaylistVideo = ReadVideo(_redis.HashGet(key, "currentVideo")),
            IsCurrentVideoRemoved = ReadBoolean(_redis.HashGet(key, "isCurrentVideoRemoved")),
            ServerTimestampUnixMs = now,
            Revision = ReadLong(_redis.HashGet(key, "revision"))
        };
    }

    public void RemoveRoom(string roomHash) => _redis.KeyDelete(Key(roomHash));

    private double GetEffectiveTime(RedisKey key, long now)
    {
        var currentTime = ReadDouble(_redis.HashGet(key, "currentTime"));
        if (!ReadBoolean(_redis.HashGet(key, "isPlaying")))
        {
            return currentTime;
        }

        var startedAt = ReadLong(_redis.HashGet(key, "serverTimestampUnixMs"));
        return currentTime + Math.Max(0, now - startedAt) / 1000d;
    }

    private static PlaylistVideo? ReadVideo(RedisValue value)
    {
        var json = value.ToString();
        return string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<PlaylistVideo>(json, JsonOptions);
    }

    private static bool ReadBoolean(RedisValue value)
    {
        var text = value.ToString();
        return text == "1" || (bool.TryParse(text, out var parsed) && parsed);
    }

    private static double ReadDouble(RedisValue value) => double.TryParse(value.ToString(), out var parsed) ? parsed : 0;

    private static long ReadLong(RedisValue value) => long.TryParse(value.ToString(), out var parsed) ? parsed : 0;

    private static RedisKey Key(string roomHash) => $"shared-watch:playback:{Uri.EscapeDataString(roomHash)}";
}



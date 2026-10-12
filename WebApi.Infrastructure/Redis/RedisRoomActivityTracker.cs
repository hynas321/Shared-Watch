using StackExchange.Redis;
using WebApi.Application.Services;
namespace WebApi.Infrastructure.Redis;
public sealed class RedisRoomActivityTracker : IRoomActivityTracker
{
    private static readonly TimeSpan ActivityKeyLifetime = TimeSpan.FromHours(2);
    private readonly IDatabase _redis;
    public RedisRoomActivityTracker(IConnectionMultiplexer redis) => _redis = redis.GetDatabase();
    public Task TouchAsync(string roomHash) => _redis.StringSetAsync(Key(roomHash), DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), ActivityKeyLifetime);
    public async Task<bool> IsInactiveAsync(string roomHash, TimeSpan inactivityPeriod)
    {
        var value = await _redis.StringGetAsync(Key(roomHash));
        return value.HasValue && long.TryParse(value.ToString(), out var last) && DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - last >= inactivityPeriod.TotalMilliseconds;
    }
    public Task RemoveAsync(string roomHash) => _redis.KeyDeleteAsync(Key(roomHash));
    private static RedisKey Key(string roomHash) => $"shared-watch:rooms:activity:{Uri.EscapeDataString(roomHash)}";
}


using StackExchange.Redis;
using WebApi.Application.Services;
namespace WebApi.Infrastructure.Redis;
public sealed class RedisHubConnectionRegistry : IHubConnectionRegistry
{
    private readonly IDatabase _redis;
    public RedisHubConnectionRegistry(IConnectionMultiplexer redis) => _redis = redis.GetDatabase();
    public void AddUserConnection(string roomHash, string userId, string connectionId)
    {
        _redis.SetAdd(UserKey(roomHash, userId), connectionId);
        _redis.StringSet(ConnectionKey(roomHash, connectionId), userId);
    }
    public bool RefreshUserConnection(string roomHash, string userId, string connectionId)
    {
        var key = ConnectionKey(roomHash, connectionId);

        if (_redis.StringGet(key) != userId) return false;

        return true;
    }
    public bool RemoveUserConnection(string roomHash, string userId, string connectionId)
    {
        var key = UserKey(roomHash, userId);
        var removed = _redis.SetRemove(key, connectionId);
        _redis.KeyDelete(ConnectionKey(roomHash, connectionId));

        if (_redis.SetLength(key) == 0) _redis.KeyDelete(key);

        return removed;
    }
    public IReadOnlyCollection<string> GetConnectionIdsByUserId(string roomHash, string userId)
    {
        var key = UserKey(roomHash, userId);
        var ids = _redis.SetMembers(key).Select(value => value.ToString()).Where(value => value.Length > 0).ToList();
        var active = new List<string>(ids.Count);

        foreach (var id in ids)
        {
            if (_redis.KeyExists(ConnectionKey(roomHash, id))) active.Add(id); else _redis.SetRemove(key, id);
        }
        return active;
    }
    public string? GetUserIdByConnectionId(string roomHash, string connectionId)
    {
        var value = _redis.StringGet(ConnectionKey(roomHash, connectionId));
        return value.HasValue ? value.ToString() : null;
    }
    private static RedisKey UserKey(string room, string user) => $"shared-watch:presence:{Uri.EscapeDataString(room)}:user:{Uri.EscapeDataString(user)}";
    private static RedisKey ConnectionKey(string room, string connection) => $"shared-watch:presence:{Uri.EscapeDataString(room)}:connection:{Uri.EscapeDataString(connection)}";
}


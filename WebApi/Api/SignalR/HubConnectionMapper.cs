using System.Collections.Concurrent;
using WebApi.Api.SignalR.Interfaces;

namespace WebApi.Api.SignalR
{
    public class HubConnectionMapper : IHubConnectionMapper
    {
        private static ConcurrentDictionary<string, string> _userConnections { get; } = new ConcurrentDictionary<string, string>();

        public bool AddUserConnection(string userId, string connectionId)
        {
            return _userConnections.TryAdd(userId, connectionId);
        }

        public bool RemoveUserConnection(string userId, string connectionId)
        {
            return _userConnections.TryRemove(userId, out _);
        }

        public List<string> GetAllUserIds()
        {
            return _userConnections.Keys.ToList();
        }

        public List<string> GetAllConnectionIds()
        {
            return _userConnections.Values.ToList();
        }

        public string GetConnectionIdByUserId(string userId)
        {
            return _userConnections.TryGetValue(userId, out var connectionId) ? connectionId : null;
        }

        public string GetUserIdByConnectionId(string connectionId)
        {
            return _userConnections.FirstOrDefault(pair => pair.Value == connectionId).Key;
        }

        public bool IsUserConnected(string userId)
        {
            return _userConnections.ContainsKey(userId);
        }

        public bool IsConnectionActive(string connectionId)
        {
            return _userConnections.Values.Contains(connectionId);
        }
    }
}

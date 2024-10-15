using System.Collections.Concurrent;
using WebApi.Api.SignalR.Interfaces;

namespace WebApi.Api.SignalR
{
    public class HubConnectionMapper : IHubConnectionMapper
    {
        private static ConcurrentDictionary<string, List<string>> _userConnections { get; } = new ConcurrentDictionary<string, List<string>>();
        private static ConcurrentDictionary<string, CancellationTokenSource> _pendingDisconnections { get; } = new ConcurrentDictionary<string, CancellationTokenSource>();

        public bool AddUserConnection(string userId, string connectionId)
        {
            if (_pendingDisconnections.TryRemove(userId, out var cancellationTokenSource))
            {
                cancellationTokenSource.Cancel();
            }

            _userConnections.AddOrUpdate(
                userId,
                new List<string> { connectionId },
                (key, existingConnections) =>
                {
                    existingConnections.Add(connectionId);
                    return existingConnections;
                });

            return true;
        }

        public bool RemoveUserConnection(string userId, string connectionId)
        {
            if (_userConnections.TryGetValue(userId, out var connections))
            {
                connections.Remove(connectionId);
                if (connections.Count == 0)
                {
                    _userConnections.TryRemove(userId, out _);
                }
                return true;
            }
            return false;
        }

        public string GetUserIdByConnectionId(string connectionId)
        {
            return _userConnections.FirstOrDefault(pair => pair.Value.Contains(connectionId)).Key;
        }

        public List<string> GetConnectionIdsByUserId(string userId)
        {
            return _userConnections.TryGetValue(userId, out var connectionIds) ? connectionIds : new List<string>();
        }

        public bool IsUserConnected(string userId)
        {
            return _userConnections.ContainsKey(userId) && _userConnections[userId].Any();
        }

        public void TrackPendingDisconnection(string userId, CancellationTokenSource cts)
        {
            _pendingDisconnections[userId] = cts;
        }

        public void ClearPendingDisconnection(string userId)
        {
            _pendingDisconnections.TryRemove(userId, out _);
        }
    }
}

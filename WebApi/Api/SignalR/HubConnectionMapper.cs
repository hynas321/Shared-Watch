using System.Collections.Concurrent;
using WebApi.Api.SignalR.Interfaces;

namespace WebApi.Api.SignalR
{
    public class HubConnectionMapper : IHubConnectionMapper
    {
        private static ConcurrentDictionary<string, List<string>> _userConnections = new ConcurrentDictionary<string, List<string>>();
        private static ConcurrentDictionary<string, ConcurrentDictionary<string, CancellationTokenSource>> _pendingDisconnections = new ConcurrentDictionary<string, ConcurrentDictionary<string, CancellationTokenSource>>();

        public bool AddUserConnection(string userId, string connectionId)
        {
            CancelPendingDisconnection(userId, connectionId);

            _userConnections.AddOrUpdate(
                userId,
                new List<string> { connectionId },
                (key, existingConnections) =>
                {
                    lock (existingConnections)
                    {
                        if (!existingConnections.Contains(connectionId))
                        {
                            existingConnections.Add(connectionId);
                        }
                    }
                    return existingConnections;
                });

            return true;
        }

        public bool RemoveUserConnection(string userId, string connectionId)
        {
            if (_userConnections.TryGetValue(userId, out var connections))
            {
                lock (connections)
                {
                    connections.Remove(connectionId);
                    if (connections.Count == 0)
                    {
                        _userConnections.TryRemove(userId, out _);
                    }
                }
                return true;
            }
            return false;
        }

        public string GetUserIdByConnectionId(string connectionId)
        {
            foreach (var pair in _userConnections)
            {
                if (pair.Value.Contains(connectionId))
                {
                    return pair.Key;
                }
            }
            return null;
        }

        public List<string> GetConnectionIdsByUserId(string userId)
        {
            return _userConnections.TryGetValue(userId, out var connectionIds) ? new List<string>(connectionIds) : new List<string>();
        }

        public void TrackPendingDisconnection(string userId, string connectionId, CancellationTokenSource cts)
        {
            var userPendingDisconnections = _pendingDisconnections.GetOrAdd(userId, new ConcurrentDictionary<string, CancellationTokenSource>());
            userPendingDisconnections[connectionId] = cts;
        }

        public void CancelPendingDisconnection(string userId, string connectionId)
        {
            if (_pendingDisconnections.TryGetValue(userId, out var userPendingDisconnections))
            {
                if (userPendingDisconnections.TryRemove(connectionId, out var cts))
                {
                    cts.Cancel();
                }
            }
        }

        public bool IsConnectionPending(string userId, string connectionId)
        {
            return _pendingDisconnections.TryGetValue(userId, out var userPendingDisconnections) && userPendingDisconnections.ContainsKey(connectionId);
        }

        public void ClearPendingDisconnection(string userId, string connectionId)
        {
            if (_pendingDisconnections.TryGetValue(userId, out var userPendingDisconnections))
            {
                userPendingDisconnections.TryRemove(connectionId, out _);
                if (userPendingDisconnections.IsEmpty)
                {
                    _pendingDisconnections.TryRemove(userId, out _);
                }
            }
        }
    }
}

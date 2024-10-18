using System.Threading;

namespace WebApi.Api.SignalR.Interfaces
{
    public interface IHubConnectionMapper
    {
        bool AddUserConnection(string userId, string connectionId);
        bool RemoveUserConnection(string userId, string connectionId);
        string GetUserIdByConnectionId(string connectionId);
        List<string> GetConnectionIdsByUserId(string userId);

        void TrackPendingDisconnection(string userId, string connectionId, CancellationTokenSource cts);
        void CancelPendingDisconnection(string userId, string connectionId);
        bool IsConnectionPending(string userId, string connectionId);
        void ClearPendingDisconnection(string userId, string connectionId);
    }
}
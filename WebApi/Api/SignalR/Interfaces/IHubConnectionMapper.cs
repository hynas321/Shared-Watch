namespace WebApi.Api.SignalR.Interfaces;

public interface IHubConnectionMapper
{
    bool AddUserConnection(string userId, string connectionId);
    bool RemoveUserConnection(string userId, string connectionId);
    string GetUserIdByConnectionId(string connectionId);
    List<string> GetConnectionIdsByUserId(string userId);
    bool IsUserConnected(string userId);
    void TrackPendingDisconnection(string userId, CancellationTokenSource cts);
    void ClearPendingDisconnection(string userId);
}

namespace WebApi.Application.Services;
public interface IHubConnectionRegistry
{
    void AddUserConnection(string roomHash, string userId, string connectionId);
    bool RefreshUserConnection(string roomHash, string userId, string connectionId);
    bool RemoveUserConnection(string roomHash, string userId, string connectionId);
    IReadOnlyCollection<string> GetConnectionIdsByUserId(string roomHash, string userId);
    string? GetUserIdByConnectionId(string roomHash, string connectionId);
}

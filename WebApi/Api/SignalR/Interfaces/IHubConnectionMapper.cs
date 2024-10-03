namespace WebApi.Api.SignalR.Interfaces
{
    public interface IHubConnectionMapper
    {
        public bool AddUserConnection(string userId, string connectionId);

        public bool RemoveUserConnection(string userId, string connectionId);
        public List<string> GetAllUserIds();

        public List<string> GetAllConnectionIds();

        public string GetConnectionIdByUserId(string userId);
        string GetUserIdByConnectionId(string connectionId);

        public bool IsUserConnected(string userId);

        public bool IsConnectionActive(string connectionId);
    }
}

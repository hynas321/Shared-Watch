public interface IUser
{
    string Username { get; set; }
    bool IsAdmin { get; set; }
    string AuthorizationToken { get; set; }
    string SignalRConnectionId { get; set; }
}
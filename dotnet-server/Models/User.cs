using System.Runtime.InteropServices;

public class User {
    public string Username { get; set; }
    public bool IsAdmin { get; set; }
    public string AuthorizationToken { get; set; }
    public string SignalRConnectionId { get; set; }

    public User(string username, [Optional] bool isAdmin, [Optional] string authorizationToken, [Optional] string signalRConnectionId)
    {
        Username = username;
        IsAdmin = isAdmin;
        AuthorizationToken = authorizationToken ?? Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8);
        SignalRConnectionId = signalRConnectionId;
    }
}
using System.Runtime.InteropServices;

public class User {
    public string Username { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsInRoom { get; set; }
    public string AuthorizationToken { get; set; }

    public User(string username, [Optional] bool isAdmin, [Optional] bool isInRoom, [Optional] string authorizationToken)
    {
        Username = username;
        IsAdmin = isAdmin;
        IsInRoom = isInRoom;
        AuthorizationToken = authorizationToken ?? Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8);
    }
}
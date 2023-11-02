using System.Runtime.InteropServices;

public class User {
    public string Username { get; set; }
    public bool IsAdmin { get; set; }
    public string AccessToken { get; set; }

    public User(string username, [Optional] bool isAdmin)
    {
        Username = username;
        IsAdmin = isAdmin;
        AccessToken = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8);
    }
}
public class User {
    public string Username { get; set; }
    public bool IsAdmin { get; set; }
    public string AccessToken { get; set; }

    public User(string username)
    {
        Username = username;
        IsAdmin = false;
        AccessToken = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8);
    }
}
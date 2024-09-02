namespace DotnetServer.Api.DTO;

public class UserDTO {
    public string Username { get; set; }
    public bool IsAdmin { get; set; }

    public UserDTO() { }

    public UserDTO(string username, bool isAdmin)
    {
        Username = username;
        IsAdmin = isAdmin;
    }
}
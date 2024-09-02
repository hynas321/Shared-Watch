using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace DotnetServer.Core.Entities;

public class User
{
    [Key]
    public string AuthorizationToken { get; set; }

    [Required]
    public string Username { get; set; }
    public bool IsAdmin { get; set; }
    public string SignalRConnectionId { get; set; }

    // Foreign Key
    public string RoomHash { get; set; }

    public User() { }

    public User(string username, bool isAdmin = false, string authorizationToken = null, string signalRConnectionId = null)
    {
        Username = username;
        IsAdmin = isAdmin;
        AuthorizationToken = authorizationToken ?? Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8);
        SignalRConnectionId = signalRConnectionId;
    }
}
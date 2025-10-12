using System.ComponentModel.DataAnnotations;

namespace WebApi.Core.Entities;

public class User
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Username { get; set; } = string.Empty;
    [Required]
    public string Role { get; set; } = string.Empty;


    // Foreign Key
    public string? RoomHash { get; set; }

    public User() { }

    public User(string username, string role)
    {
        Username = username;
        Role = role;
    }
}
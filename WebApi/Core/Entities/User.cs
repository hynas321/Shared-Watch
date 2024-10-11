using System.ComponentModel.DataAnnotations;
using WebApi.Application.Constants;

namespace WebApi.Core.Entities;

public class User
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Username { get; set; }
    [Required]
    public string Role { get; set; }


    // Foreign Key
    public string RoomHash { get; set; }

    public User() { }

    public User(string username, string role)
    {
        Username = username;
        Role = role;
    }
}
using WebApi.Application.Constants;
using WebApi.Core.Entities;
namespace WebApi.Application.DTO;
public class UserDTO
{
    public string? Username { get; set; }
    public bool IsAdmin { get; set; }
    public UserDTO() { }
    public UserDTO(string? username, bool isAdmin) { Username = username; IsAdmin = isAdmin; }
    public static UserDTO FromEntity(User user) => new(user.Username, user.Role == Role.Admin);
}

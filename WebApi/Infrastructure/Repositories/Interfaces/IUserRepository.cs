using WebApi.Api.DTO;
using WebApi.Core.Entities;

namespace WebApi.Infrastructure.Repositories;

public interface IUserRepository
{
    Task<bool> AddUserAsync(string roomHash, User user);
    Task<User> DeleteUserByUsernameAsync(string roomHash, string username);
    Task<User> DeleteUserByConnectionIdAsync(string roomHash, string connectionId);
    Task<bool> UpdateUserAsync(User user);
    Task<User> GetUserAsync(string roomHash, string username);
    Task<IEnumerable<UserDTO>> GetUsersDTOAsync(string roomHash);
}
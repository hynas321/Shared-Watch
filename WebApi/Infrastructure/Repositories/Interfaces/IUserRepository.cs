using WebApi.Api.DTO;
using WebApi.Core.Entities;

namespace WebApi.Infrastructure.Repositories;

public interface IUserRepository
{
    Task<bool> AddUserAsync(string roomHash, User user);
    Task<User> DeleteUserAsync(string roomHash, string authorizationToken);
    Task<(User user, string roomHash)> DeleteUserAsync(string connectionId);
    Task<bool> UpdateUserAsync(User user);
    Task<User> GetUserByAuthorizationTokenAsync(string roomHash, string authorizationToken);
    Task<User> GetUserByAuthorizationTokenAsync(string authorizationToken);
    Task<User> GetUserByUsernameAsync(string roomHash, string username);
    Task<User> GetUserByConnectionIdAsync(string connectionId);
    Task<IEnumerable<UserDTO>> GetUsersDTOAsync(string roomHash);
}
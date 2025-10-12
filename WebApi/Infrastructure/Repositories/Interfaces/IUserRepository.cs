using WebApi.Api.DTO;
using WebApi.Core.Entities;

namespace WebApi.Infrastructure.Repositories;

public interface IUserRepository
{
    Task<bool> AddUserAsync(string roomHash, User user, CancellationToken cancellationToken);
    Task<User?> DeleteUserByUsernameAsync(string roomHash, string username, CancellationToken cancellationToken);
    Task<User?> DeleteUserByConnectionIdAsync(string roomHash, string connectionId, CancellationToken cancellationToken);
    Task<bool> UpdateUserAsync(User user, CancellationToken cancellationToken);
    Task<User?> GetUserAsync(string roomHash, string username, CancellationToken cancellationToken);
    Task<IEnumerable<UserDTO>> GetUsersDTOAsync(string roomHash, CancellationToken cancellationToken);
}
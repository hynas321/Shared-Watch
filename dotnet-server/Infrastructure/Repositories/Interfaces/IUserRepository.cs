using DotnetServer.Api.DTO;
using DotnetServer.Core.Entities;

namespace DotnetServer.Infrastructure.Repositories;

public interface IUserRepository
{
    bool AddUser(string roomHash, User user);
    User DeleteUser(string roomHash, string authorizationToken);
    (User user, string roomHash) DeleteUser(string connectionId);
    User GetUserByAuthorizationToken(string roomHash, string authorizationToken);
    User GetUserByAuthorizationToken(string authorizationToken);
    User GetUserByUsername(string roomHash, string username);
    User GetUserByConnectionId(string connectionId);
    IEnumerable<UserDTO> GetUsersDTO(string roomHash);
}
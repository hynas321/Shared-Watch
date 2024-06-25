public interface IUserRepository
{
    bool AddUser(string roomHash, IUser user);
    IUser DeleteUser(string roomHash, string authorizationToken);
    (IUser user, string roomHash) DeleteUserByConnectionId(string connectionId);
    IUser GetUserByAuthorizationToken(string roomHash, string authorizationToken);
    IUser GetUserByAuthorizationToken(string authorizationToken);
    IUser GetUserByUsername(string roomHash, string username);
    IUser GetUserByConnectionId(string connectionId);
    IEnumerable<UserDTO> GetUsersDTO(string roomHash);
}
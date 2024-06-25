public class UserRepository : IUserRepository
{
    private readonly IRoomRepository _roomRepository;

    public UserRepository(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public bool AddUser(string roomHash, IUser user)
    {
        var room = _roomRepository.GetRoom(roomHash);

        if (room == null) return false;

        room.Users = room.Users.Concat([user]);

        return true;
    }

    public IUser DeleteUser(string roomHash, string authorizationToken)
    {
        var room = _roomRepository.GetRoom(roomHash);

        if (room == null) return null;

        var user = room.Users.FirstOrDefault(u => u.AuthorizationToken == authorizationToken);

        if (user == null) return null;

        room.Users = room.Users.Where(u => u.AuthorizationToken != authorizationToken);

        return user;
    }

    public (IUser user, string roomHash) DeleteUserByConnectionId(string connectionId)
    {
        foreach (var room in _roomRepository.GetRooms())
        {
            var userToRemove = room.Users.FirstOrDefault(user => user.SignalRConnectionId == connectionId);
            if (userToRemove != null)
            {
                room.Users = room.Users.Where(u => u.SignalRConnectionId != connectionId).ToList();
                return (userToRemove, room.RoomHash);
            }
        }

        return (null, null);
    }

    public IUser GetUserByAuthorizationToken(string roomHash, string authorizationToken)
    {
        var room = _roomRepository.GetRoom(roomHash);
        return room?.Users.FirstOrDefault(u => u.AuthorizationToken == authorizationToken);
    }

    public IUser GetUserByAuthorizationToken(string authorizationToken)
    {
        foreach (var room in _roomRepository.GetRooms())
        {
            foreach (var user in room.Users)
            {
                if (user.AuthorizationToken == authorizationToken)
                {
                    return user;
                }
            }
        }

        return null;
    }

    public IUser GetUserByUsername(string roomHash, string username)
    {
        var room = _roomRepository.GetRoom(roomHash);
        return room?.Users.FirstOrDefault(u => u.Username == username);
    }

    public IUser GetUserByConnectionId(string connectionId)
    {
        foreach (var room in _roomRepository.GetRooms())
        {
            foreach (var user in room.Users)
            {
                if (user.SignalRConnectionId == connectionId)
                {
                    return user;
                }
            }
        }

        return null;
    }

    public IEnumerable<UserDTO> GetUsersDTO(string roomHash)
    {
        var room = _roomRepository.GetRoom(roomHash);
        return room?.Users.Select(user => new UserDTO(
            user.Username, user.IsAdmin
        ));
    }
}
public class UserManager {
    private readonly RoomManager _roomManager = new RoomManager();

    public bool AddUser(string roomHash, User user)
    {
        try
        {
            Room room = _roomManager.GetRoom(roomHash);

            if (room == null)
            {
                return false;
            }

            room.Users.Add(user);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public User DeleteUser(string roomHash, string authorizationToken)
    {
        try
        {
            Room room = _roomManager.GetRoom(roomHash);

            if (room == null)
            {
                return null;
            }

            User user = room.Users.FirstOrDefault(u => u.AuthorizationToken == authorizationToken);

            if (user == null)
            {
                return null;
            }

            room.Users.Remove(user);

            return user;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public (User user, string roomHash) DeleteUserByConnectionId(string connectionId)
    {
        try
        {
            IEnumerable<Room> rooms = _roomManager.GetRooms();

            foreach (var room in rooms)
            {
                foreach (var user in room.Users)
                {
                    if (user.SignalRConnectionId == connectionId)
                    {
                        room.Users.Remove(user);
                        return (user, room.RoomHash);
                    }
                }
            }

            return (null, null);
        }
        catch (Exception)
        {
            return (null, null);
        }
    }
    public User GetUserByAuthorizationToken(string roomHash, string authorizationToken)
    {
        try
        {
            Room room = _roomManager.GetRoom(roomHash);

            if (room == null)
            {
                return null;
            }

            User user = room.Users.FirstOrDefault(u => u.AuthorizationToken == authorizationToken);

            if (user == null)
            {
                return null;
            }

            return user;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public User GetUserByAuthorizationToken(string authorizationToken)
    {
        try
        {
            IEnumerable<Room> rooms = _roomManager.GetRooms();

            foreach (var room in rooms)
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
        catch (Exception)
        {
            return null;
        }
    }

    public User GetUserByUsername(string roomHash, string username)
    {
        try
        {
            Room room = _roomManager.GetRoom(roomHash);

            if (room == null)
            {
                return null;
            }

            User user = room.Users.FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                return null;
            }

            return user;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public User GetUserByConnectionId(string connectionId)
    {
        try
        {
            IEnumerable<Room> rooms = _roomManager.GetRooms();

            foreach (var room in rooms)
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
        catch (Exception)
        {
            return null;
        }
    }

    public IEnumerable<UserDTO> GetUsersDTO(string roomHash)
    {
        Room room = _roomManager.GetRoom(roomHash);

        foreach (var user in room.Users)
        {
            UserDTO userDTO = new UserDTO(
                user.Username,
                user.IsAdmin
            );

            yield return userDTO;
        }
    }
}
public class RoomManager {
    private static List<Room> rooms = new List<Room>();

    public bool AddRoom(Room room)
    {
        try
        {
            bool roomExists = rooms.Any(r => r.RoomName == room.RoomName);

            if (roomExists)
            {
                return false;
            }

            rooms.Add(room);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public Room RemoveRoom(string roomHash)
    {
        try
        {
            Room room = GetRoom(roomHash);

            if (room == null)
            {
                return null;
            }

            rooms.Remove(room);

            return room;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public Room GetRoom(string roomHash)
    {
        return rooms.FirstOrDefault(r => r.RoomHash == roomHash) ?? null;
    }

    public IEnumerable<Room> GetAllRooms()
    {
        return rooms;
    }

    public IEnumerable<RoomDTO> GetAllRoomsDTO()
    {
        foreach (var room in rooms)
        {
            RoomDTO roomDTO = new RoomDTO(
                room.RoomHash,
                room.RoomName,
                room.RoomPassword == "" ? RoomTypesEnum.Public : RoomTypesEnum.Private,
                room.Users.Count,
                room.RoomSettings.MaxUsers
            );

            yield return roomDTO;
        }
    }

    public User GetUser(string roomHash, string authorizationToken)
    {
        try
        {
            Room room = GetRoom(roomHash);

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

    public bool AddUser(string roomHash, User user)
    {
        try
        {
            Room room = rooms.FirstOrDefault(r => r.RoomHash == roomHash);

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
            Room room = rooms.FirstOrDefault(r => r.RoomHash == roomHash);

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
}
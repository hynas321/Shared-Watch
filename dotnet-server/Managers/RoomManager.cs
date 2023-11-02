public class RoomManager {
    private static List<Room> rooms = new List<Room>();

    public bool InsertRoom(string roomName, string roomPassword)
    {
        try
        {   
            Room newRoom = new Room(roomName, roomPassword);

            rooms.Add(newRoom);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public Room GetRoom(string roomHash)
    {
        return rooms.FirstOrDefault(r => r.RoomHash == roomHash) ?? null;
    }

    public User GetUser(string roomHash, string accessToken)
    {
        Room room = GetRoom(roomHash);

        if (room == null)
        {
            return null;
        }

        User user = room.Users.FirstOrDefault(u => u.AccessToken == accessToken);

        if (user == null)
        {
            return null;
        }

        return user;
    }

    public bool InsertUser(string roomHash, User user)
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

    public User DeleteUser(string roomHash, string accessToken)
    {
        try
        {
            Room room = rooms.FirstOrDefault(r => r.RoomHash == roomHash);

            if (room == null)
            {
                return null;
            }

            User user = room.Users.FirstOrDefault(u => u.AccessToken == accessToken);

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
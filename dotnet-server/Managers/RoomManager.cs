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

    public Room DeleteRoom(string roomHash)
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
                room.UserPermissions.MaxUsers
            );

            yield return roomDTO;
        }
    }

    public User GetUserByAuthorizationToken(string roomHash, string authorizationToken)
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

        public User GetUserByUsername(string roomHash, string username)
    {
        try
        {
            Room room = GetRoom(roomHash);

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

    public IEnumerable<UserDTO> GetUsersDTO(string roomHash)
    {
        Room room = GetRoom(roomHash);

        foreach (var user in room.Users)
        {
            UserDTO userDTO = new UserDTO(
                user.Username,
                user.IsAdmin
            );

            yield return userDTO;
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

    public bool AddChatMessage(string roomHash, ChatMessage chatMessage)
    {
        try
        {
            Room room = rooms.FirstOrDefault(r => r.RoomHash == roomHash);

            if (room == null)
            {
                return false;
            }

            room.ChatMessages.Add(chatMessage);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public bool AddPlaylistVideo(string roomHash, PlaylistVideo playlistVideo)
    {
        try
        {
            Room room = rooms.FirstOrDefault(r => r.RoomHash == roomHash);

            if (room == null)
            {
                return false;
            }

            room.PlaylistVideos.Add(playlistVideo);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public PlaylistVideo DeletePlaylistVideo(string roomHash, int playlistVideoIndex)
    {
        try
        {
            Room room = rooms.FirstOrDefault(r => r.RoomHash == roomHash);

            if (room == null)
            {
                return null;
            }

            PlaylistVideo playlistVideo = room.PlaylistVideos[playlistVideoIndex];

            if (playlistVideo == null)
            {
                return null;
            }

            room.PlaylistVideos.RemoveAt(playlistVideoIndex);

            return playlistVideo;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
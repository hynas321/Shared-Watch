public class RoomManager {
    private static List<Room> rooms = new List<Room>();

    public bool AddRoom(Room room)
    {
        try
        {
            bool roomExists = rooms.Any(r => r.RoomSettings.RoomName == room.RoomSettings.RoomName);

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

    public IEnumerable<Room> GetRooms()
    {
        return rooms;
    }

    public IEnumerable<RoomDTO> GetRoomsDTO()
    {
        foreach (var room in rooms)
        {
            RoomDTO roomDTO = new RoomDTO(
                room.RoomHash,
                room.RoomSettings.RoomName,
                room.RoomSettings.RoomPassword == "" ? RoomTypesEnum.Public : RoomTypesEnum.Private,
                room.Users.Count,
                room.RoomSettings.MaxUsers
            );

            yield return roomDTO;
        }
    }
}
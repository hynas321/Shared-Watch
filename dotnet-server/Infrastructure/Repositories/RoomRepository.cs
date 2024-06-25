public class RoomRepository : IRoomRepository
{
    private static List<IRoom> rooms = new List<IRoom>();

    public bool AddRoom(IRoom room)
    {
        bool roomExists = rooms.Any(r => r.RoomSettings.RoomName == room.RoomSettings.RoomName);

        if (roomExists) return false;

        rooms.Add(room);

        return true;
    }

    public IRoom DeleteRoom(string roomHash)
    {
        IRoom room = GetRoom(roomHash);

        if (room == null) return null;

        rooms.Remove(room);

        return room;
    }

    public IRoom GetRoom(string roomHash)
    {
        return rooms.FirstOrDefault(r => r.RoomHash == roomHash);
    }

    public IEnumerable<IRoom> GetRooms()
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
                room.RoomSettings.RoomPassword == "" ? RoomTypes.Public : RoomTypes.Private,
                room.Users.Count(),
                room.RoomSettings.MaxUsers
            );

            yield return roomDTO;
        }
    }
}
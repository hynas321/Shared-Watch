using dotnet_server.Infrastructure;

public class RoomRepository : IRoomRepository
{
    private readonly AppData _appData;

    public RoomRepository(AppData appData)
    {
        _appData = appData;
    }

    public bool AddRoom(Room room)
    {
        bool roomExists = _appData.Rooms.Any(r => r.RoomSettings.RoomName == room.RoomSettings.RoomName);

        if (roomExists)
        {
            return false;
        }

        _appData.Rooms.Add(room);

        return true;
    }

    public Room DeleteRoom(string roomHash)
    {
        Room room = GetRoom(roomHash);

        if (room == null)
        {
            return null;
        }

        _appData.Rooms.Remove(room);

        return room;
    }

    public Room GetRoom(string roomHash)
    {
        return _appData.Rooms.FirstOrDefault(r => r.RoomHash == roomHash);
    }

    public List<Room> GetRooms()
    {
        return _appData.Rooms;
    }

    public IEnumerable<RoomDTO> GetRoomsDTO()
    {
        foreach (var room in _appData.Rooms)
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
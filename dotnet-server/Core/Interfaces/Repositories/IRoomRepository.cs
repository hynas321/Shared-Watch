public interface IRoomRepository
{
    bool AddRoom(Room room);
    Room DeleteRoom(string roomHash);
    Room GetRoom(string roomHash);
    List<Room> GetRooms();
    IEnumerable<RoomDTO> GetRoomsDTO();
}
public interface IRoomRepository
{
    bool AddRoom(IRoom room);
    IRoom DeleteRoom(string roomHash);
    IRoom GetRoom(string roomHash);
    IEnumerable<IRoom> GetRooms();
    IEnumerable<RoomDTO> GetRoomsDTO();
}
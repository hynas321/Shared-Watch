public class ChatRepository : IChatRepository
{
    private readonly IRoomRepository _roomRepository;

    public ChatRepository(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public bool AddChatMessage(string roomHash, IChatMessage chatMessage)
    {
        IRoom room = _roomRepository.GetRoom(roomHash);

        if (room == null) return false;

        room.ChatMessages = room.ChatMessages.Concat([chatMessage]);

        return true;
    }
}
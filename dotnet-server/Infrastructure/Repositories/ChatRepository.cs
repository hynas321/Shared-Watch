public class ChatRepository : IChatRepository
{
    private readonly IRoomRepository _roomRepository;

    public ChatRepository(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public bool AddChatMessage(string roomHash, ChatMessage chatMessage)
    {
        Room room = _roomRepository.GetRoom(roomHash);

        if (room == null)
        {
            return false;
        }

        room.ChatMessages.Add(chatMessage);

        return true;
    }
}
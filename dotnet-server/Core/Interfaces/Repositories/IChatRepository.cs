public interface IChatRepository
{
    bool AddChatMessage(string roomHash, IChatMessage chatMessage);
}
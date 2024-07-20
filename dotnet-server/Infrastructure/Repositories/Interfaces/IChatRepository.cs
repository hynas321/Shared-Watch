public interface IChatRepository
{
    bool AddChatMessage(string roomHash, ChatMessage chatMessage);
}
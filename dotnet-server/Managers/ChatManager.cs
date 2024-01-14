public class ChatManager {
    private readonly RoomManager _roomManager = new RoomManager();

    public bool AddChatMessage(string roomHash, ChatMessage chatMessage)
    {
        try
        {
            Room room = _roomManager.GetRoom(roomHash);

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
}
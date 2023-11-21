using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class RoomHub : Hub
{
    [HubMethodName(HubEvents.AddChatMessage)]
    public async Task AddChatMessage(string roomHash, string authorizationToken, ChatMessage chatMessage)
    {
        Room room = _roomManager.GetRoom(roomHash);

        if (room == null)
        {
            _logger.LogInformation($"{roomHash} AddChatMessage: Room does not exist. Authorization Token: {authorizationToken}");
            return;
        }

        User user = room.GetUser(authorizationToken);

        if (user == null)
        {
            _logger.LogInformation($"{roomHash} AddChatMessage: User does not exist. Authorization Token: {authorizationToken}");
            return;
        }

        if (user.IsAdmin == false && room.RoomSettings.IsSendingChatMessagesAllowed == false)
        {
            _logger.LogInformation($"{roomHash} AddChatMessage: User does not have the permission. Authorization Token: {authorizationToken}");
            return;
        }


        _logger.LogInformation($"{roomHash} AddChatMessage: {chatMessage.Date} {chatMessage.Username}: {chatMessage.Text}. Authorization Token: {authorizationToken}");

        bool isChatMessageAdded = _roomManager.AddChatMessage(roomHash, chatMessage);

        if (!isChatMessageAdded)
        {
            _logger.LogInformation($"{roomHash} AddChatMessage: Error when adding a chat message. Authorization Token: {authorizationToken}");
            return;
        }

        await Clients.Group(roomHash).SendAsync(HubEvents.OnAddChatMessage, JsonHelper.Serialize(chatMessage));
    }
}
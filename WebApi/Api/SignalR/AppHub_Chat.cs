using WebApi.Core.Entities;
using WebApi.Shared.Helpers;
using Microsoft.AspNetCore.SignalR;

namespace WebApi.SignalR;

public partial class AppHub : Hub
{
    [HubMethodName(HubMessages.AddChatMessage)]
    public async Task AddChatMessageAsync(string roomHash, string authorizationToken, ChatMessage chatMessage)
    {
        try
        {
            const int maxChatMessages = 30;
            const int maxChatMessageTextLength = 200;

            Room room = await _roomRepository.GetRoomAsync(roomHash);

            if (room == null)
            {
                _logger.LogInformation($"{roomHash} AddChatMessage: Room does not exist. Authorization Token: {authorizationToken}");
                return;
            }

            User user = room.Users.FirstOrDefault(x => x.AuthorizationToken == authorizationToken);

            if (user == null)
            {
                _logger.LogInformation($"{roomHash} AddChatMessage: User does not exist. Authorization Token: {authorizationToken}");
                return;
            }

            if (!user.IsAdmin && !room.UserPermissions.CanAddChatMessage)
            {
                _logger.LogInformation($"{roomHash} AddChatMessage: User does not have permission. Authorization Token: {authorizationToken}");
                return;
            }

            if (room.ChatMessages.Count >= maxChatMessages)
            {
                ChatMessage oldestMessage = room.ChatMessages.FirstOrDefault();
                if (oldestMessage != null)
                {
                    room.ChatMessages.Remove(oldestMessage);
                }
            }

            if (chatMessage.Text.Length > maxChatMessageTextLength)
            {
                _logger.LogInformation($"{roomHash} AddChatMessage: Maximum message length reached. Authorization Token: {authorizationToken}");
                return;
            }

            bool isChatMessageAdded = await _chatRepository.AddChatMessageAsync(roomHash, chatMessage);

            if (!isChatMessageAdded)
            {
                _logger.LogInformation($"{roomHash} AddChatMessage: Error when adding a chat message. Authorization Token: {authorizationToken}");
                return;
            }

            await Clients.Group(roomHash).SendAsync(HubMessages.OnAddChatMessage, JsonHelper.Serialize(chatMessage));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
        }
    }
}
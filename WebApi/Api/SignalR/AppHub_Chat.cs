using WebApi.Core.Entities;
using WebApi.Shared.Helpers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using WebApi.Application.Constants;
using System.Security.Claims;

namespace WebApi.SignalR;

public partial class AppHub : Hub
{
    const int maxChatMessages = 30;
    const int maxChatMessageTextLength = 200;

    [Authorize]
    [HubMethodName(HubMessages.AddChatMessage)]
    public async Task AddChatMessageAsync(string roomHash, ChatMessage chatMessage)
    {
        try
        {
            var room = await _roomRepository.GetRoomAsync(roomHash);

            if (room == null)
            {
                _logger.LogInformation($"{roomHash} AddChatMessage: Room does not exist. User identifier: {Context.UserIdentifier}");
                return;
            }

            var role = Context.User?.FindFirstValue(ClaimTypes.Role);

            if (role != Role.Admin && !room.UserPermissions.CanAddChatMessage)
            {
                _logger.LogInformation($"{roomHash} AddChatMessage: User does not have permission. User identifier: {Context.UserIdentifier}");
                return;
            }

            if (room.ChatMessages.Count >= maxChatMessages)
            {
                var oldestMessage = room.ChatMessages.FirstOrDefault();

                if (oldestMessage != null)
                {
                    room.ChatMessages.Remove(oldestMessage);
                }
            }

            if (chatMessage.Text.Length > maxChatMessageTextLength)
            {
                _logger.LogInformation($"{roomHash} AddChatMessage: Maximum message length reached. User identifier: {Context.UserIdentifier}");
                return;
            }

            var isChatMessageAdded = await _chatRepository.AddChatMessageAsync(roomHash, chatMessage);

            if (!isChatMessageAdded)
            {
                _logger.LogInformation($"{roomHash} AddChatMessage: Error when adding a chat message. User identifier: {Context.UserIdentifier}");
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
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class AppHub : Hub
{
    [HubMethodName(HubMessages.AddChatMessage)]
    public async Task AddChatMessage(string roomHash, string authorizationToken, ChatMessage chatMessage)
    {
        try
        {
            IRoom room = _roomRepository.GetRoom(roomHash);

            if (room == null)
            {
                _logger.LogInformation(
                    $"{roomHash} AddChatMessage: Room does not exist. Authorization Token: {authorizationToken}"
                );

                return;
            }

            IUser user = room.Users.FirstOrDefault(x => x.AuthorizationToken == authorizationToken);

            if (user == null)
            {
                _logger.LogInformation(
                    $"{roomHash} AddChatMessage: User does not exist. Authorization Token: {authorizationToken}"
                );

                return;
            }

            if (user.IsAdmin == false && room.UserPermissions.CanAddChatMessage == false)
            {
                _logger.LogInformation(
                    $"{roomHash} AddChatMessage: User does not have the permission. Authorization Token: {authorizationToken}"
                );

                return;
            }

            int maxChatMessageTextLength = 200;

            if (chatMessage.Text.Length > maxChatMessageTextLength)
            {
                _logger.LogInformation(
                    $"{roomHash} AddChatMessage: Maximum length of a message reached. Authorization Token: {authorizationToken}"
                );

                return;
            }

            DateTime utcNow = DateTime.UtcNow.AddDays(0);
            TimeZoneInfo localTimeZone = TimeZoneInfo.Local;
            DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, localTimeZone);

            chatMessage.Date = localTime.ToString("HH:mm:ss");

            _logger.LogInformation(
                $"{roomHash} AddChatMessage: {chatMessage.Date} {chatMessage.Username}: {chatMessage.Text}. Authorization Token: {authorizationToken}"
            );

            bool isChatMessageAdded = _chatRepository.AddChatMessage(roomHash, chatMessage);

            if (!isChatMessageAdded)
            {
                _logger.LogInformation(
                    $"{roomHash} AddChatMessage: Error when adding a chat message. Authorization Token: {authorizationToken}"
                );

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
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class RoomHub : Hub
{
    [HubMethodName(HubEvents.AddChatMessage)]
    public async Task AddChatMessage(string roomHash, ChatMessage chatMessage)
    {
        _logger.LogInformation($"{roomHash} AddChatMessage: {chatMessage.Date} {chatMessage.Username}: {chatMessage.Text}");

        await Clients.All.SendAsync(HubEvents.OnAddChatMessage, JsonHelper.Serialize(chatMessage));
    }
}
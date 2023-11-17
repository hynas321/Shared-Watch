using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class RoomHub : Hub
{
    [HubMethodName(HubEvents.JoinRoom)]
    public async Task JoinRoom(string roomHash, string username)
    {   
        try
        {
            Room room = _roomManager.GetRoom(roomHash);

            if (room == null)
            {
                return;
            }

            User user = new User(username);
            UserDTO userDTO = new UserDTO(user.Username, user.IsAdmin);

            _roomManager.AddUser(room.RoomHash, user);
            
            await Clients.GroupExcept(roomHash, Context.ConnectionId).SendAsync(HubEvents.OnJoinRoom, JsonHelper.Serialize(userDTO));
            await Clients.Client(Context.ConnectionId).SendAsync(HubEvents.OnJoinRoom, JsonHelper.Serialize(userDTO));
        }
        catch (Exception ex)
        {
            _logger.LogError(Convert.ToString(ex));
        }
    }

    [HubMethodName(HubEvents.LeaveRoom)]
    public async Task LeaveRoom(string roomHash, string authorizationToken)
    {
        try
        {
            Room room = _roomManager.GetRoom(roomHash);

            if (room == null)
            {
                return;
            }

            User deletedUser = _roomManager.DeleteUser(roomHash, authorizationToken);
            UserDTO deletedUserDTO = new UserDTO(deletedUser.Username, deletedUser.IsAdmin);

            await Clients.GroupExcept(roomHash, Context.ConnectionId).SendAsync(HubEvents.OnJoinRoom, JsonHelper.Serialize(deletedUserDTO));
        }
        catch (Exception ex)
        {
            _logger.LogError(Convert.ToString(ex));
        }
    }
}
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class RoomHubConnection : Hub
{
    [HubMethodName(HubMethods.JoinRoom)]
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
            
            await Clients.GroupExcept(roomHash, Context.ConnectionId).SendAsync(HubMethods.OnJoinRoom, JsonHelper.Serialize(userDTO));
            await Clients.Client(Context.ConnectionId).SendAsync(HubMethods.OnJoinRoom, JsonHelper.Serialize(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(Convert.ToString(ex));
        }
    }

    [HubMethodName(HubMethods.LeaveRoom)]
    public async Task LeaveRoom(string roomHash, string accessToken)
    {
        try
        {
            Room room = _roomManager.GetRoom(roomHash);

            if (room == null)
            {
                return;
            }

            User deletedUser = _roomManager.DeleteUser(roomHash, accessToken);
            UserDTO deletedUserDTO = new UserDTO(deletedUser.Username, deletedUser.IsAdmin);

            await Clients.GroupExcept(roomHash, Context.ConnectionId).SendAsync(HubMethods.OnJoinRoom, JsonHelper.Serialize(deletedUserDTO));
        }
        catch (Exception ex)
        {
            _logger.LogError(Convert.ToString(ex));
        }
    }
}
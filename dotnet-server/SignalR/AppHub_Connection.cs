using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class AppHub : Hub
{
    private readonly ILogger<AppHub> _logger;
    private readonly RoomManager _roomManager = new RoomManager();
    private readonly ChatManager _chatManager = new ChatManager();
    private readonly PlaylistManager _playlistManager = new PlaylistManager();
    private readonly UserManager _userManager = new UserManager();
    private readonly PlaylistService _playlistService;
    private readonly YouTubeAPIService _youtubeAPIService;
    private readonly IHubContext<AppHub> _roomHubContext;

    public AppHub(
        ILogger<AppHub> logger,
        PlaylistService playlistHandler,
        YouTubeAPIService youTubeAPIService,
        IHubContext<AppHub> roomHubContext
    )
    {
        _logger = logger;
        _playlistService = playlistHandler;
        _youtubeAPIService = youTubeAPIService;
        _roomHubContext = roomHubContext;
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Client(Context.ConnectionId).SendAsync(HubEvents.OnReceiveConnectionId, Context.ConnectionId);
        _logger.LogInformation($"Hub: User Connected: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        (User removedUser, string roomHash) = _userManager.DeleteUserByConnectionId(Context.ConnectionId);
        _logger.LogInformation(Context.ConnectionId);
        if (removedUser != null && roomHash != null)
        {
            UserDTO userDTO = new UserDTO(removedUser.Username, removedUser.IsAdmin);
            await _roomHubContext.Clients.Group(roomHash).SendAsync(HubEvents.OnLeaveRoom, userDTO);

            Room room = _roomManager.GetRoom(roomHash);

            if (room.Users.Count == 0)
            {
                _roomManager.DeleteRoom(roomHash);

                IEnumerable<RoomDTO> rooms = _roomManager.GetRoomsDTO();
        
                await _roomHubContext.Clients.All.SendAsync(HubEvents.OnListOfRoomsUpdated, JsonHelper.Serialize(rooms));
            }
        }

        _logger.LogInformation($"Hub: User Disconnected: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }
}
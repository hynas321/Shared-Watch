using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class RoomHub : Hub
{
    private readonly ILogger<RoomHub> _logger;
    private readonly RoomManager _roomManager = new RoomManager();
    private readonly ChatManager _chatManager = new ChatManager();
    private readonly PlaylistManager _playlistManager = new PlaylistManager();
    private readonly UserManager _userManager = new UserManager();
    private readonly PlaylistService _playlistService;
    private readonly YouTubeAPIService _youtubeAPIService;
    private readonly IHubContext<RoomHub> _roomHubContext;

    public RoomHub(
        ILogger<RoomHub> logger,
        PlaylistService playlistHandler,
        YouTubeAPIService youTubeAPIService,
        IHubContext<RoomHub> roomHubContext
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
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {   
        string signalRConnectionId = Context.ConnectionId;

        (User removedUser, string roomHash) = _userManager.DeleteUserByConnectionId(signalRConnectionId);

        if (removedUser == null || roomHash == null)
        {
            return;
        }

        UserDTO userDTO = new UserDTO(removedUser.Username, removedUser.IsAdmin);

        await _roomHubContext.Clients.Group(roomHash).SendAsync(HubEvents.OnLeaveRoom, userDTO);
        await base.OnDisconnectedAsync(exception);
    }
}
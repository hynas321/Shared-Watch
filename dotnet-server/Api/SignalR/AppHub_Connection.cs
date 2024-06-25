using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class AppHub : Hub
{
    private readonly ILogger<AppHub> _logger;
    private readonly IRoomRepository _roomRepository;
    private readonly IChatRepository _chatRepository;
    private readonly IPlaylistRepository _playlistRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPlaylistService _playlistService;
    private readonly IYouTubeAPIService _youtubeAPIService;
    private readonly IHubContext<AppHub> _roomHubContext;

    public AppHub(
        ILogger<AppHub> logger,
        IPlaylistService playlistHandler,
        IYouTubeAPIService youTubeAPIService,
        IHubContext<AppHub> roomHubContext
    )
    {
        _logger = logger;
        _playlistService = playlistHandler;
        _youtubeAPIService = youTubeAPIService;
        _roomHubContext = roomHubContext;

        _roomRepository = new RoomRepository();
        _chatRepository = new ChatRepository(_roomRepository);
        _playlistRepository = new PlaylistRepository(_roomRepository);
        _userRepository = new UserRepository(_roomRepository);
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            await Clients.Client(Context.ConnectionId).SendAsync(HubMessages.OnReceiveConnectionId, Context.ConnectionId);
            _logger.LogInformation($"Hub: User Connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
        }
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        try
        {
            (IUser removedUser, string roomHash) = _userRepository.DeleteUserByConnectionId(Context.ConnectionId);

            if (removedUser != null && roomHash != null)
            {
                UserDTO userDTO = new UserDTO(removedUser.Username, removedUser.IsAdmin);
                await _roomHubContext.Clients.Group(roomHash).SendAsync(HubMessages.OnLeaveRoom, userDTO);

                IRoom room = _roomRepository.GetRoom(roomHash);

                if (!room.Users.Any())
                {
                    _roomRepository.DeleteRoom(roomHash);

                    IEnumerable<RoomDTO> rooms = _roomRepository.GetRoomsDTO();
            
                    await _roomHubContext.Clients.All.SendAsync(HubMessages.OnListOfRoomsUpdated, JsonHelper.Serialize(rooms));
                }
            }

            _logger.LogInformation($"Hub: User Disconnected: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
        }
    }
}
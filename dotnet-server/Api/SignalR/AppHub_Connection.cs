using AutoMapper;
using DotnetServer.Application.Services.Interfaces;
using DotnetServer.Api.DTO;
using DotnetServer.Core.Entities;
using DotnetServer.Infrastructure.Repositories;
using DotnetServer.Shared.Helpers;
using Microsoft.AspNetCore.SignalR;

namespace DotnetServer.SignalR;

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
    private readonly IMapper _mapper;

    public AppHub(
        ILogger<AppHub> logger,
        IPlaylistService playlistHandler,
        IYouTubeAPIService youTubeAPIService,
        IHubContext<AppHub> roomHubContext,
        IRoomRepository roomRepository,
        IChatRepository chatRepository,
        IPlaylistRepository playlistRepository,
        IUserRepository userRepository,
        IMapper mapper
    )
    {
        _logger = logger;
        _playlistService = playlistHandler;
        _youtubeAPIService = youTubeAPIService;
        _roomHubContext = roomHubContext;
        _roomRepository = roomRepository;
        _chatRepository = chatRepository;
        _playlistRepository = playlistRepository;
        _userRepository = userRepository;
        _mapper = mapper;
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
            (User removedUser, string roomHash) = await _userRepository.DeleteUserAsync(Context.ConnectionId);

            if (removedUser == null || roomHash == null)
            {
                _logger.LogInformation($"Hub: User Disconnected: {Context.ConnectionId}");
                await base.OnDisconnectedAsync(exception);
                return;
            }

            UserDTO userDTO = _mapper.Map<UserDTO>(removedUser);

            await _roomHubContext.Clients.Group(roomHash).SendAsync(HubMessages.OnLeaveRoom, userDTO);

            Room room = await _roomRepository.GetRoomAsync(roomHash);

            if (room.Users.Count == 0)
            {
                await _roomRepository.DeleteRoomAsync(roomHash);

                IEnumerable<RoomDTO> rooms = await _roomRepository.GetRoomsDTOAsync();
            
                await _roomHubContext.Clients.All.SendAsync(HubMessages.OnListOfRoomsUpdated, JsonHelper.Serialize(rooms));
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
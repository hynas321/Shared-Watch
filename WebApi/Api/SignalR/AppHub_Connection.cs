using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using WebApi.Api.DTO;
using WebApi.Api.SignalR;
using WebApi.Api.SignalR.Interfaces;
using WebApi.Application.Services.Interfaces;
using WebApi.Infrastructure.Repositories;

namespace WebApi.SignalR
{
    public partial class AppHub : Hub
    {
        private readonly ILogger<AppHub> _logger;
        private readonly IRoomRepository _roomRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IPlaylistRepository _playlistRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPlaylistService _playlistService;
        private readonly IYouTubeAPIService _youtubeAPIService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IVideoPlayerStateService _videoPlayerStateService;
        private readonly IHubConnectionMapper _hubConnectionMapper;
        private readonly IMapper _mapper;

        public AppHub(
            ILogger<AppHub> logger,
            IPlaylistService playlistHandler,
            IYouTubeAPIService youTubeAPIService,
            IRoomRepository roomRepository,
            IChatRepository chatRepository,
            IPlaylistRepository playlistRepository,
            IUserRepository userRepository,
            IJwtTokenService tokenService,
            IVideoPlayerStateService videoPlayerStateService,
            IHubConnectionMapper hubConnectionMapper,
            IMapper mapper)
        {
            _logger = logger;
            _playlistService = playlistHandler;
            _youtubeAPIService = youTubeAPIService;
            _roomRepository = roomRepository;
            _chatRepository = chatRepository;
            _playlistRepository = playlistRepository;
            _userRepository = userRepository;
            _jwtTokenService = tokenService;
            _videoPlayerStateService = videoPlayerStateService;
            _hubConnectionMapper = hubConnectionMapper;
            _mapper = mapper;
        }

        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            var userId = GetUserId();
            var roomHash = GetRoomHash();

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("A user connected without a valid user ID.");
                await base.OnConnectedAsync();
                return;
            }

            _logger.LogInformation("User connected: {UserId}", userId);

            _hubConnectionMapper.AddUserConnection(userId, connectionId);
            await Groups.AddToGroupAsync(connectionId, roomHash);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = GetUserId();
            var connectionId = Context.ConnectionId;
            var roomHash = GetRoomHash();

            if (string.IsNullOrEmpty(userId))
            {
                await base.OnDisconnectedAsync(exception);
                return;
            }

            var disconnectCancellationTokenSource = new CancellationTokenSource();
            _hubConnectionMapper.TrackPendingDisconnection(userId, disconnectCancellationTokenSource);

            try
            {
                await Task.Delay(3500, disconnectCancellationTokenSource.Token);

                var remainingConnections = _hubConnectionMapper.GetConnectionIdsByUserId(userId);

                if (remainingConnections.Count == 1)
                {
                    var removedUser = await _userRepository.DeleteUserByConnectionIdAsync(roomHash, connectionId);

                    _hubConnectionMapper.RemoveUserConnection(userId, connectionId);
                    await Groups.RemoveFromGroupAsync(connectionId, roomHash);

                    var room = await _roomRepository.GetRoomAsync(roomHash);

                    if (room != null && room.Users.Count == 0)
                    {
                        await _roomRepository.DeleteRoomAsync(roomHash);
                    }

                    if (removedUser == null)
                    {
                        return;
                    }

                    var userDTO = _mapper.Map<UserDTO>(removedUser);
                    await Clients.Group(roomHash).SendAsync(HubMessages.OnLeaveRoom, userDTO);

                    _logger.LogInformation("Hub: User {UserId} Disconnected: {ConnectionId}", userId, connectionId);
                }
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("Hub: Disconnection canceled for user {UserId}, connection reestablished", userId);
            }
            finally
            {
                _hubConnectionMapper.ClearPendingDisconnection(userId);
                await base.OnDisconnectedAsync(exception);
            }
        }


        private string GetUserId()
        {
            return Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        private string GetRoomHash()
        {
            return Context.User?.FindFirst(ClaimTypes.Hash)?.Value;
        }
    }
}

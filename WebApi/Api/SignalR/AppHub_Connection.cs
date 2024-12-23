using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using WebApi.Api.DTO;
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
        private readonly IVideoPlayerService _playlistService;
        private readonly IYouTubeAPIService _youtubeAPIService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IVideoPlayerStateService _videoPlayerStateService;
        private readonly IHubConnectionMapper _hubConnectionMapper;
        private readonly IMapper _mapper;

        public AppHub(
            ILogger<AppHub> logger,
            IVideoPlayerService playlistHandler,
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

            _logger.LogInformation("User connected: {UserId} with ConnectionId: {ConnectionId}", userId, connectionId);

            var previousConnections = _hubConnectionMapper.GetConnectionIdsByUserId(userId);

            foreach (var prevConnectionId in previousConnections)
            {
                _hubConnectionMapper.CancelPendingDisconnection(userId, prevConnectionId);
            }

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
            _hubConnectionMapper.TrackPendingDisconnection(userId, connectionId, disconnectCancellationTokenSource);

            try
            {
                await Task.Delay(5000, disconnectCancellationTokenSource.Token);

                if (_hubConnectionMapper.IsConnectionPending(userId, connectionId))
                {
                    var remainingConnections = _hubConnectionMapper.GetConnectionIdsByUserId(userId);

                    if (remainingConnections.Count >= 1)
                    {
                        _logger.LogInformation("Disconnecting user {UserId}, no reconnection within timeout.", userId);

                        var removedUser = await _userRepository.DeleteUserByConnectionIdAsync(roomHash, connectionId);
                        var room = await _roomRepository.GetRoomAsync(roomHash);

                        _hubConnectionMapper.RemoveUserConnection(userId, connectionId);
                        await Groups.RemoveFromGroupAsync(connectionId, roomHash);

                        if (room != null && room.Users.Count == 0)
                        {
                            await _roomRepository.DeleteRoomAsync(roomHash);
                            _logger.LogInformation("Room {RoomHash} deleted as no users remain.", roomHash);
                        }

                        if (removedUser != null)
                        {
                            var userDTO = _mapper.Map<UserDTO>(removedUser);
                            await Clients.Group(roomHash).SendAsync(HubMessages.OnLeaveRoom, userDTO);

                            _logger.LogInformation("User {UserId} disconnected and removed from room {RoomHash}.", userId, roomHash);
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("Reconnection occurred for user {UserId}, canceling disconnection.", userId);
                }
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("Disconnection canceled, user {UserId} reconnected.", userId);
            }
            finally
            {
                _hubConnectionMapper.ClearPendingDisconnection(userId, connectionId);
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

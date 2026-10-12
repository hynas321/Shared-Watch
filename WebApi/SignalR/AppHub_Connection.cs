
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using WebApi.Application.Constants;
using WebApi.Application.DTO;
using WebApi.Application.Repositories;
using WebApi.Application.Services;
using WebApi.Application.Options;
using Microsoft.Extensions.Options;

namespace WebApi.SignalR;

[Authorize]
public partial class AppHub : Hub
    {
        private readonly ILogger<AppHub> _logger;
        private readonly IRoomRepository _roomRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IUserRepository _userRepository;
        private readonly IVideoPlayerService _playlistService;
        private readonly IPlaylistApplicationService _playlistApplicationService;
        private readonly IVideoPlayerStateService _videoPlayerStateService;
        private readonly IRoomPasswordService _roomPasswordService;
        private readonly IHubConnectionRegistry _hubConnectionRegistry;
        private readonly IRoomLifecycleService _roomLifecycleService;
        private readonly ChatOptions _chatOptions;
        private readonly IRoomActivityTracker _roomActivityTracker;
    

        public AppHub(
            ILogger<AppHub> logger,
            IVideoPlayerService playlistHandler,
            IPlaylistApplicationService playlistApplicationService,
            IRoomRepository roomRepository,
            IChatRepository chatRepository,
            IUserRepository userRepository,
            IVideoPlayerStateService videoPlayerStateService,
            IHubConnectionRegistry hubConnectionRegistry,
            IRoomPasswordService roomPasswordService,
            IRoomLifecycleService roomLifecycleService,
            IOptions<ChatOptions> chatOptions,
            IRoomActivityTracker roomActivityTracker)
        {
            _logger = logger;
            _playlistService = playlistHandler;
            _playlistApplicationService = playlistApplicationService;
            _roomRepository = roomRepository;
            _chatRepository = chatRepository;
            _userRepository = userRepository;
            _videoPlayerStateService = videoPlayerStateService;
            _hubConnectionRegistry = hubConnectionRegistry;
            _roomPasswordService = roomPasswordService;
            _roomLifecycleService = roomLifecycleService;
            _chatOptions = chatOptions.Value;
            _roomActivityTracker = roomActivityTracker;
    
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

            if (string.IsNullOrWhiteSpace(roomHash))
            {
                _logger.LogWarning("A user connected without a room hash.");
                await base.OnConnectedAsync();
                return;
            }

            _logger.LogInformation("User connected: {UserId} with ConnectionId: {ConnectionId}", userId, connectionId);

            _hubConnectionRegistry.AddUserConnection(roomHash, userId, connectionId);
            await _roomActivityTracker.TouchAsync(roomHash);
            await Groups.AddToGroupAsync(connectionId, roomHash);
            _playlistService.StartPlaylistService(roomHash);

            await base.OnConnectedAsync();
        }

        [Authorize]
        [HubMethodName(HubMessages.RefreshConnectionPresence)]
        public Task RefreshConnectionPresenceAsync(string roomHash)
        {
            var userId = GetUserId();
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var refreshed = _hubConnectionRegistry.RefreshUserConnection(roomHash, userId, Context.ConnectionId);
                _logger.LogDebug(
                    "Presence refresh for room {RoomHash}, user {UserId}, connection {ConnectionId}. Refreshed: {Refreshed}",
                    roomHash,
                    userId,
                    Context.ConnectionId,
                    refreshed);
            }
            else
            {
                _logger.LogWarning(
                    "Presence refresh ignored because the connection {ConnectionId} has no user identity. Room: {RoomHash}",
                    Context.ConnectionId,
                    roomHash);
            }

            return Task.CompletedTask;
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            var connectionId = Context.ConnectionId;
            var roomHash = GetRoomHash();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(roomHash))
            {
                _logger.LogWarning(
                    exception,
                    "SignalR connection {ConnectionId} disconnected without complete room identity. User: {UserId}, Room: {RoomHash}",
                    connectionId,
                    userId,
                    roomHash);
                await base.OnDisconnectedAsync(exception);
                return;
            }

            try
            {
                _logger.LogInformation(
                    exception,
                    "SignalR connection {ConnectionId} disconnected for user {UserId} in room {RoomHash}. Waiting for reconnect grace period.",
                    connectionId,
                    userId,
                    roomHash);
                await Task.Delay(TimeSpan.FromSeconds(3));
                var removed = _hubConnectionRegistry.RemoveUserConnection(roomHash, userId, connectionId);
                await Groups.RemoveFromGroupAsync(connectionId, roomHash);

                if (!removed)
                {
                    return;
                }

                var remainingConnections = _hubConnectionRegistry.GetConnectionIdsByUserId(roomHash, userId);
                _logger.LogDebug(
                    "Disconnected connection {ConnectionId} removed from presence: {Removed}. Remaining connections for user {UserId} in room {RoomHash}: {RemainingConnections}",
                    connectionId,
                    removed,
                    userId,
                    roomHash,
                    remainingConnections.Count);
                if (remainingConnections.Count == 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(3));
                    remainingConnections = _hubConnectionRegistry.GetConnectionIdsByUserId(roomHash, userId);
                    if (remainingConnections.Count > 0)
                    {
                        return;
                    }

                    _logger.LogInformation("Disconnecting user {UserId}, no active connections remain.", userId);

                    var removedUser = await _userRepository.DeleteUserByUsernameAsync(roomHash, userId, CancellationToken.None);
                    if (removedUser is not null)
                    {
                        await Clients.Group(roomHash).SendAsync(HubMessages.OnLeaveRoom, UserDTO.FromEntity(removedUser));
                        await PromoteNextAdminIfNeededAsync(roomHash);
                        var roomDeleted = await _roomLifecycleService.DeleteIfEmptyAsync(roomHash, CancellationToken.None);
                        if (roomDeleted)
                        {
                            _logger.LogInformation("Room {RoomHash} deleted as no users remain.", roomHash);
                        }
                    }

                    if (removedUser is not null)
                    {
                        _logger.LogInformation("User {UserId} disconnected and removed from room {RoomHash}.", userId, roomHash);
                    }
                }
            }
            finally
            {
                await base.OnDisconnectedAsync(exception);
            }
        }

        private string? GetUserId()
        {
            return Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        private string? GetRoomHash()
        {
            return Context.User?.FindFirst(ClaimTypes.Hash)?.Value;
        }

        private async Task<bool> IsCurrentUserAdminAsync(string roomHash, CancellationToken cancellationToken)
        {
            var username = GetUserId();
            if (string.IsNullOrWhiteSpace(username))
            {
                return false;
            }

            var user = await _userRepository.GetUserAsync(roomHash, username, cancellationToken);
            return user?.Role == Role.Admin;
        }

        private async Task PromoteNextAdminIfNeededAsync(string roomHash)
        {
            var room = await _roomRepository.GetRoomAsync(roomHash, CancellationToken.None);
            if (room is null || room.Users.Any(user => user.Role == Role.Admin))
            {
                return;
            }

            var nextAdmin = room.Users
                .OrderBy(user => user.Id)
                .FirstOrDefault();

            if (nextAdmin is null)
            {
                return;
            }

            nextAdmin.Role = Role.Admin;
            await _userRepository.UpdateUserAsync(nextAdmin, CancellationToken.None);

            foreach (var connectionId in _hubConnectionRegistry
                         .GetConnectionIdsByUserId(roomHash, nextAdmin.Username))
            {
                await Clients.Client(connectionId)
                    .SendAsync(HubMessages.OnReceiveJwt, CancellationToken.None);
            }

            await Clients.Group(roomHash)
                .SendAsync(HubMessages.OnSetAdminStatus, UserDTO.FromEntity(nextAdmin), CancellationToken.None);

            _logger.LogInformation(
                "Promoted user {Username} to admin in room {RoomHash} after disconnect cleanup.",
                nextAdmin.Username,
                roomHash);
        }
}



using WebApi.Application.DTO;
using WebApi.Application.Repositories;
using WebApi.Application.Services;
using WebApi.Core.Entities;
using WebApi.Core.Entities.In_memory;
using WebApi.Application.Constants;

namespace WebApi.Application.Services;

public enum JoinRoomFailure { RoomNotFound, InvalidPassword, RoomFull, UsernameTaken }
public enum LeaveRoomFailure { RoomNotFound, UserNotFound }
public sealed record JoinRoomData(
    string AuthorizationToken,
    bool IsAdmin,
    IEnumerable<ChatMessage> ChatMessages,
    IEnumerable<PlaylistVideo> PlaylistVideos,
    IEnumerable<UserDTO> Users,
    RoomSettingsDTO RoomSettings,
    UserPermissions UserPermissions,
    VideoPlayer? VideoPlayer,
    UserDTO JoinedUser);
public sealed record JoinRoomResult(JoinRoomFailure? Failure, JoinRoomData? Data = null);
public sealed record LeaveRoomResult(LeaveRoomFailure? Failure, UserDTO? User = null);

public sealed class RoomApplicationService
{
    private readonly IRoomRepository _rooms;
    private readonly IUserRepository _users;
    private readonly IJwtTokenService _tokens;
    private readonly IRoomPasswordService _passwords;
    private readonly IVideoPlayerStateService _videoState;

    public RoomApplicationService(IRoomRepository rooms, IUserRepository users, IJwtTokenService tokens,
        IRoomPasswordService passwords, IVideoPlayerStateService videoState)
    {
        _rooms = rooms;
        _users = users;
        _tokens = tokens;
        _passwords = passwords;
        _videoState = videoState;
    }

    public async Task<Room?> CreateRoomAsync(string roomName, string roomPassword, CancellationToken cancellationToken)
    {
        if (await _rooms.GetRoomByNameAsync(roomName, cancellationToken) is not null)
            return null;

        var storedPassword = string.IsNullOrEmpty(roomPassword) ? string.Empty : _passwords.Hash(roomPassword);
        var room = new Room(roomName, storedPassword);
        return await _rooms.AddRoomAsync(room, cancellationToken) ? room : null;
    }

    public async Task<JoinRoomResult> JoinAsync(string roomHash, string username, string roomPassword, CancellationToken cancellationToken)
    {
        var room = await _rooms.GetRoomAsync(roomHash, cancellationToken);
        if (room is null)
            return new(JoinRoomFailure.RoomNotFound);

        var storedPassword = room.RoomSettings.RoomPassword ?? string.Empty;
        if (storedPassword.Length > 0 && !_passwords.Verify(roomPassword, storedPassword))
            return new(JoinRoomFailure.InvalidPassword);

        if (room.Users.Any(user => string.Equals(user.Username, username, StringComparison.OrdinalIgnoreCase)))
            return new(JoinRoomFailure.UsernameTaken);
        if (room.Users.Count >= room.RoomSettings.MaxUsers)
            return new(JoinRoomFailure.RoomFull);

        var role = room.Users.Count == 0 ? Role.Admin : Role.User;
        if (!await _users.AddUserAsync(roomHash, new User(username, role), cancellationToken))
            return new(JoinRoomFailure.RoomNotFound);

        room = await _rooms.GetRoomAsync(roomHash, cancellationToken);
        if (room is null)
            return new(JoinRoomFailure.RoomNotFound);

        var joinedUser = room.Users.FirstOrDefault(user => user.Username == username);
        if (joinedUser is null)
            return new(JoinRoomFailure.RoomNotFound);

        var userDTO = UserDTO.FromEntity(joinedUser);
        var data = new JoinRoomData(
            _tokens.GenerateToken(username, role, roomHash),
            role == Role.Admin,
            room.ChatMessages.OrderBy(message => message.Date).ToList(),
            room.PlaylistVideos.ToList(),
            room.Users.Select(UserDTO.FromEntity).ToList(),
            RoomSettingsDTO.FromEntity(room.RoomSettings),
            room.UserPermissions,
            _videoState.GetVideoPlayer(roomHash),
            userDTO);
        return new(null, data);
    }

    public async Task<LeaveRoomResult> LeaveAsync(string roomHash, string username, CancellationToken cancellationToken)
    {
        if (await _rooms.GetRoomAsync(roomHash, cancellationToken) is null)
            return new(LeaveRoomFailure.RoomNotFound);

        var user = await _users.DeleteUserByUsernameAsync(roomHash, username, cancellationToken);
        return user is null
            ? new(LeaveRoomFailure.UserNotFound)
            : new(null, UserDTO.FromEntity(user));
    }
}

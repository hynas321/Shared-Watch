
using WebApi.Application.DTO;
using WebApi.Api.Contracts.Output;
using WebApi.Application.Services;
using WebApi.Application.Repositories;
using WebApi.Application.Constants;
using WebApi.Core.Enums;
using WebApi.SignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using WebApi.Api.Contracts.Input;
using Microsoft.AspNetCore.Http;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IHubContext<AppHub> _appHubContext;
    private readonly RoomApplicationService _roomApplicationService;
    private readonly IHubConnectionRegistry _hubConnectionRegistry;
    private readonly IRoomRepository _roomRepository;
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRoomLifecycleService _roomLifecycleService;
    private readonly ILogger<UserController> _logger;


    public UserController(
        IHubContext<AppHub> appHubContext,
        RoomApplicationService roomApplicationService,
        IHubConnectionRegistry hubConnectionRegistry,
        IRoomRepository roomRepository,
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        IRoomLifecycleService roomLifecycleService,
        ILogger<UserController> logger)
    {
        _appHubContext = appHubContext;
        _roomApplicationService = roomApplicationService;
        _hubConnectionRegistry = hubConnectionRegistry;
        _roomRepository = roomRepository;
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _roomLifecycleService = roomLifecycleService;
        _logger = logger;
    }

    [HttpPost("Join/{roomHash}")]
    public async Task<IActionResult> JoinRoom([FromBody] RoomJoinInput input, [FromRoute] string roomHash, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Join requested for room {RoomHash} by user {Username}. PasswordProvided: {PasswordProvided}, RequestId: {RequestId}",
            roomHash,
            input.Username,
            !string.IsNullOrEmpty(input.RoomPassword),
            HttpContext.TraceIdentifier);

        if (!ModelState.IsValid || string.IsNullOrEmpty(roomHash))
        {
            _logger.LogWarning("Join validation failed for room {RoomHash} and user {Username}. RequestId: {RequestId}", roomHash, input.Username, HttpContext.TraceIdentifier);
            return BadRequest();
        }

        var result = await _roomApplicationService.JoinAsync(
            roomHash,
            input.Username!,
            input.RoomPassword!,
            cancellationToken);
        if (result.Failure is not null)
        {
            _logger.LogWarning("Join rejected for room {RoomHash} and user {Username}. Reason: {Failure}. RequestId: {RequestId}", roomHash, input.Username, result.Failure, HttpContext.TraceIdentifier);
            return result.Failure switch
            {
                JoinRoomFailure.RoomNotFound => NotFound(),
                JoinRoomFailure.InvalidPassword => Unauthorized(),
                JoinRoomFailure.RoomFull => Forbid(),
                JoinRoomFailure.UsernameTaken => Conflict(),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        var data = result.Data!;
        SetAccessTokenCookie(data.AuthorizationToken);
        var output = new RoomJoinOutput
        {
            IsAdmin = data.IsAdmin,
            ChatMessages = data.ChatMessages.ToList(),
            PlaylistVideos = data.PlaylistVideos.ToList(),
            Users = data.Users.ToList(),
            RoomSettings = data.RoomSettings,
            UserPermissions = data.UserPermissions,
            VideoPlayer = data.VideoPlayer
        };

        await _appHubContext.Clients.Group(roomHash)
            .SendAsync(HubMessages.OnJoinRoom, data.JoinedUser, cancellationToken);

        _logger.LogInformation("User {Username} joined room {RoomHash}. IsAdmin: {IsAdmin}, RequestId: {RequestId}", input.Username, roomHash, data.IsAdmin, HttpContext.TraceIdentifier);

        return Ok(output);
    }

    [Authorize]
    [HttpPost("RefreshToken")]
    public async Task<IActionResult> RefreshToken(CancellationToken cancellationToken)
    {
        var username = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var roomHash = User.FindFirstValue(ClaimTypes.Hash);
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(roomHash))
        {
            return Unauthorized();
        }

        var user = await _userRepository.GetUserAsync(roomHash, username, cancellationToken);
        if (user is null)
        {
            return Unauthorized();
        }

        SetAccessTokenCookie(_jwtTokenService.GenerateToken(username, user.Role.ToString(), roomHash));
        return NoContent();
    }

    [Authorize]
    [HttpDelete("Leave/{roomHash}")]
    public async Task<IActionResult> LeaveRoom([FromRoute] string roomHash, CancellationToken cancellationToken)
    {
        var userIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);

        _logger.LogInformation("Leave requested for room {RoomHash} by user {Username}. RequestId: {RequestId}", roomHash, userIdentifier, HttpContext.TraceIdentifier);

        if (userIdentifier is null)
        {
            return Unauthorized();
        }

        if (string.IsNullOrEmpty(roomHash))
        {
            return BadRequest();
        }

        var result = await _roomApplicationService.LeaveAsync(roomHash, userIdentifier, cancellationToken);
        if (result.Failure is not null)
        {
            _logger.LogWarning("Leave rejected for room {RoomHash} and user {Username}. Reason: {Failure}. RequestId: {RequestId}", roomHash, userIdentifier, result.Failure, HttpContext.TraceIdentifier);
            return result.Failure switch
            {
                LeaveRoomFailure.RoomNotFound => NotFound(),
                LeaveRoomFailure.UserNotFound => Unauthorized(),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        var connectionIds = _hubConnectionRegistry
            .GetConnectionIdsByUserId(roomHash, userIdentifier)
            .ToList();

        foreach (var connectionId in connectionIds)
        {
            _hubConnectionRegistry.RemoveUserConnection(
                roomHash,
                userIdentifier,
                connectionId);
        }

        await _appHubContext.Clients.Group(roomHash)
            .SendAsync(HubMessages.OnLeaveRoom, result.User, cancellationToken);

        await PromoteNextAdminIfNeededAsync(roomHash, cancellationToken);

        var roomDeleted = await _roomLifecycleService.DeleteIfEmptyAsync(roomHash, cancellationToken);
        if (roomDeleted)
        {
            _logger.LogInformation(
                "Room {RoomHash} deleted immediately after its last user left. RequestId: {RequestId}",
                roomHash,
                HttpContext.TraceIdentifier);
        }

        _logger.LogInformation("User {Username} left room {RoomHash}; removed {ConnectionCount} presence connections. RequestId: {RequestId}", userIdentifier, roomHash, connectionIds.Count, HttpContext.TraceIdentifier);

        Response.Cookies.Delete(AuthenticationConstants.AccessTokenCookie);
        return Ok();
    }

    private async Task PromoteNextAdminIfNeededAsync(
        string roomHash,
        CancellationToken cancellationToken)
    {
        var room = await _roomRepository.GetRoomAsync(roomHash, cancellationToken);
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
        await _userRepository.UpdateUserAsync(nextAdmin, cancellationToken);

        foreach (var connectionId in _hubConnectionRegistry
                     .GetConnectionIdsByUserId(roomHash, nextAdmin.Username))
        {
            await _appHubContext.Clients.Client(connectionId)
                .SendAsync(HubMessages.OnReceiveJwt, cancellationToken);
        }

        await _appHubContext.Clients.Group(roomHash)
            .SendAsync(HubMessages.OnSetAdminStatus, UserDTO.FromEntity(nextAdmin), cancellationToken);

        _logger.LogInformation(
            "Promoted user {Username} to admin in room {RoomHash} after no admin remained.",
            nextAdmin.Username,
            roomHash);
    }

    private void SetAccessTokenCookie(string token)
    {
        Response.Cookies.Append(
            AuthenticationConstants.AccessTokenCookie,
            token,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                MaxAge = TimeSpan.FromHours(12)
            });
    }
}

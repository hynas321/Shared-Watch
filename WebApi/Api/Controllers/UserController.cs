using AutoMapper;
using WebApi.Api.DTO;
using WebApi.Api.HttpClasses.Input;
using WebApi.Api.HttpClasses.Output;
using WebApi.Core.Entities;
using WebApi.Infrastructure.Repositories;
using WebApi.Shared.Helpers;
using WebApi.SignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace WebApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IHubContext<AppHub> _appHubContext;
    private readonly IRoomRepository _roomRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UserController(
        ILogger<UserController> logger,
        IHubContext<AppHub> appHubContext,
        IRoomRepository roomRepository,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _logger = logger;
        _appHubContext = appHubContext;
        _roomRepository = roomRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    [HttpPost("Join/{roomHash}")]
    public async Task<IActionResult> JoinRoom(
        [FromBody] RoomJoinInput input,
        [FromRoute] string roomHash,
        [FromHeader(Name = "X-SignalR-ConnectionId")] string signalRConnectionId = null,
        [FromHeader(Name = "Authorization")] string authorizationToken = null)
    {
        if (!ModelState.IsValid || string.IsNullOrEmpty(roomHash) || string.IsNullOrEmpty(signalRConnectionId))
        {
            _logger.LogInformation($"{roomHash} Join: BadRequest. RoomPassword: {input.RoomPassword}, Username: {input.Username}");
            return BadRequest();
        }

        Room room = await _roomRepository.GetRoomAsync(roomHash);

        if (room == null)
        {
            _logger.LogInformation($"{roomHash} Join: NotFound. RoomPassword: {input.RoomPassword}, Username: {input.Username}");
            return NotFound();
        }

        if (room.RoomSettings.RoomPassword != input.RoomPassword)
        {
            _logger.LogInformation($"{roomHash} Join: Unauthorized. RoomPassword: {input.RoomPassword}, Username: {input.Username}");
            return Unauthorized();
        }

        if (room.RoomSettings.MaxUsers == room.Users.Count)
        {
            _logger.LogInformation($"{roomHash} Join: Forbidden. RoomPassword: {input.RoomPassword}, Username: {input.Username}");
            return Forbid();
        }

        if (room.Users.Any(u => u.Username == input.Username) || room.Users.Any(u => u.AuthorizationToken == authorizationToken))
        {
            _logger.LogInformation($"{roomHash} Join: Conflict. RoomPassword: {input.RoomPassword}, Username: {input.Username}");
            return Conflict();
        }

        bool isUserAdmin = room.Users.Count == 0 || room.AdminTokens.Any(o => o == authorizationToken);
        User newUser = new User(input.Username, isUserAdmin, authorizationToken, signalRConnectionId);

        UserDTO newUserDTO = _mapper.Map<UserDTO>(newUser);

        if (room.Users.Count == 0)
        {
            room.AdminTokens.Add(newUser.AuthorizationToken);
        }

        await _appHubContext.Groups.AddToGroupAsync(signalRConnectionId, roomHash);

        bool isNewUserAdded = await _userRepository.AddUserAsync(roomHash, newUser);

        if (!isNewUserAdded)
        {
            _logger.LogInformation($"{roomHash} Join: InternalServerError. RoomPassword: {input.RoomPassword}, Username: {input.Username}");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        RoomJoinOutput output = new RoomJoinOutput
        {
            AuthorizationToken = newUser.AuthorizationToken,
            IsAdmin = newUser.IsAdmin,
            ChatMessages = room.ChatMessages.ToList(),
            PlaylistVideos = room.PlaylistVideos.ToList(),
            Users = (await _userRepository.GetUsersDTOAsync(roomHash)).ToList(),
            RoomSettings = room.RoomSettings,
            UserPermissions = room.UserPermissions,
            VideoPlayer = room.VideoPlayer
        };

        var serializedOutput = JsonHelper.Serialize(output);

        await _appHubContext.Clients.Group(roomHash).SendAsync(HubMessages.OnJoinRoom, newUserDTO);

        IEnumerable<RoomDTO> rooms = await _roomRepository.GetRoomsDTOAsync();
        await _appHubContext.Clients.All.SendAsync(HubMessages.OnListOfRoomsUpdated, JsonHelper.Serialize(rooms));

        _logger.LogInformation($"{roomHash} Join: OK. RoomPassword: {input.RoomPassword}, Username: {input.Username}");
        return Ok(serializedOutput);
    }

    [HttpDelete("Leave/{roomHash}")]
    public async Task<IActionResult> Leave(
        [FromRoute] string roomHash,
        [FromHeader(Name = "X-SignalR-ConnectionId")] string signalRConnectionId = null,
        [FromHeader(Name = "Authorization")] string authorizationToken = null)
    {
        if (string.IsNullOrEmpty(roomHash) || string.IsNullOrEmpty(authorizationToken) || string.IsNullOrEmpty(signalRConnectionId))
        {
            _logger.LogInformation($"{roomHash} Leave: BadRequest. AuthorizationToken: {authorizationToken}");
            return BadRequest();
        }

        Room room = await _roomRepository.GetRoomAsync(roomHash);

        if (room == null)
        {
            _logger.LogInformation($"{roomHash} Leave: NotFound. AuthorizationToken: {authorizationToken}");
            return NotFound();
        }

        User user = await _userRepository.GetUserByAuthorizationTokenAsync(roomHash, authorizationToken);

        if (user == null)
        {
            _logger.LogInformation($"{roomHash} Leave: Unauthorized. AuthorizationToken: {authorizationToken}");
            return Unauthorized();
        }

        User deletedUser = await _userRepository.DeleteUserAsync(roomHash, authorizationToken);

        if (deletedUser == null)
        {
            _logger.LogInformation($"{roomHash} Leave: InternalServerError. AuthorizationToken: {authorizationToken}");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        Room updatedRoom = await _roomRepository.GetRoomAsync(roomHash);

        if (updatedRoom.Users.Count == 0)
        {
            _logger.LogInformation($"Room {roomHash} has been deleted");
            await _roomRepository.DeleteRoomAsync(roomHash);
        }

        await _appHubContext.Groups.RemoveFromGroupAsync(signalRConnectionId, roomHash);

        UserDTO userDTO = _mapper.Map<UserDTO>(user);
        await _appHubContext.Clients.Group(roomHash).SendAsync(HubMessages.OnLeaveRoom, userDTO);

        IEnumerable<RoomDTO> rooms = await _roomRepository.GetRoomsDTOAsync();
        await _appHubContext.Clients.All.SendAsync(HubMessages.OnListOfRoomsUpdated, JsonHelper.Serialize(rooms));

        _logger.LogInformation($"{roomHash} Leave: OK. AuthorizationToken: {authorizationToken}");
        return Ok();
    }
}
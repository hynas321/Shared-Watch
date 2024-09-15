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
using WebApi.Application.Services.Interfaces;
using System.Security.Claims;
using WebApi.Application.Constants;
using Microsoft.AspNetCore.Authorization;
using WebApi.Api.Services.Interfaces;

namespace WebApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IHubContext<AppHub> _appHubContext;
    private readonly IRoomRepository _roomRepository;
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _tokenService;
    private readonly IMapper _mapper;

    public UserController(
        ILogger<UserController> logger,
        IHubContext<AppHub> appHubContext,
        IRoomRepository roomRepository,
        IUserRepository userRepository,
        IJwtTokenService tokenService,
        IMapper mapper)
    {
        _logger = logger;
        _appHubContext = appHubContext;
        _roomRepository = roomRepository;
        _userRepository = userRepository;
        _tokenService = tokenService;
        _mapper = mapper;
    }

    [HttpPost("Join/{roomHash}")]
    public async Task<IActionResult> JoinRoom([FromBody] RoomJoinInput input, [FromRoute] string roomHash)
    {
        if (!ModelState.IsValid || string.IsNullOrEmpty(roomHash))
        {
            return BadRequest();
        }

        var room = await _roomRepository.GetRoomAsync(roomHash);

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

        if (room.Users.Any(u => u.Username == input.Username))
        {
            _logger.LogInformation($"{roomHash} Join: Conflict. RoomPassword: {input.RoomPassword}, Username: {input.Username}");
            return Conflict();
        }

        var userRole = room.Users.Count == 0 ? Role.Admin : Role.User;
        var newUser = new User(input.Username, userRole);

        var jwt = _tokenService.GenerateToken(
            input.Username,
            userRole,
            roomHash
        );

        var newUserDTO = _mapper.Map<UserDTO>(newUser);
        var isNewUserAdded = await _userRepository.AddUserAsync(roomHash, newUser);

        if (!isNewUserAdded)
        {
            _logger.LogInformation($"{roomHash} Join: InternalServerError. RoomPassword: {input.RoomPassword}, Username: {input.Username}");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        var output = new RoomJoinOutput
        {
            AuthorizationToken = jwt,
            IsAdmin = userRole == Role.Admin,
            ChatMessages = room.ChatMessages.ToList(),
            PlaylistVideos = room.PlaylistVideos.ToList(),
            Users = (await _userRepository.GetUsersDTOAsync(roomHash)).ToList(),
            RoomSettings = room.RoomSettings,
            UserPermissions = room.UserPermissions,
            VideoPlayer = room.VideoPlayer
        };

        var serializedOutput = JsonHelper.Serialize(output);

        await _appHubContext.Clients.Group(roomHash).SendAsync(HubMessages.OnJoinRoom, newUserDTO);

        var rooms = await _roomRepository.GetRoomsDTOAsync();

        _logger.LogInformation($"{roomHash} Join: OK. RoomPassword: {input.RoomPassword}, Username: {input.Username}");
        return Ok(serializedOutput);
    }

    [Authorize]
    [HttpDelete("Leave/{roomHash}")]
    public async Task<IActionResult> LeaveRoom([FromRoute] string roomHash)
    {
        var userIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(roomHash))
        {
            _logger.LogInformation($"{roomHash} Leave: BadRequest. User Identifier: {userIdentifier}");
            return BadRequest();
        }

        var room = await _roomRepository.GetRoomAsync(roomHash);

        if (room == null)
        {
            _logger.LogInformation($"{roomHash} Leave: NotFound. User Identifier: {userIdentifier}");
            return NotFound();
        }

        var user = await _userRepository.GetUserAsync(roomHash, userIdentifier);

        if (user == null)
        {
            _logger.LogInformation($"{roomHash} Leave: Unauthorized. User Identifier: {userIdentifier}");
            return Unauthorized();
        }

        var deletedUser = await _userRepository.DeleteUserByUsernameAsync(roomHash, userIdentifier);

        if (deletedUser == null)
        {
            _logger.LogInformation($"{roomHash} Leave: InternalServerError. User Identifier: {userIdentifier}");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        var updatedRoom = await _roomRepository.GetRoomAsync(roomHash);

        if (updatedRoom.Users.Count == 0)
        {
            _logger.LogInformation($"Room {roomHash} has been deleted");
            await _roomRepository.DeleteRoomAsync(roomHash);
        }

        var userDTO = _mapper.Map<UserDTO>(user);
        await _appHubContext.Clients.Group(roomHash).SendAsync(HubMessages.OnLeaveRoom, userDTO);

        var rooms = await _roomRepository.GetRoomsDTOAsync();

        _logger.LogInformation($"{roomHash} Leave: OK. User Identifier: {userIdentifier}");
        return Ok();
    }
}
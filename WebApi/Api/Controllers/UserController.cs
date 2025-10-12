using AutoMapper;
using WebApi.Api.DTO;
using WebApi.Api.HttpClasses.Input;
using WebApi.Api.HttpClasses.Output;
using WebApi.Core.Entities;
using WebApi.Infrastructure.Repositories;
using WebApi.SignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebApi.Application.Services.Interfaces;
using System.Security.Claims;
using WebApi.Application.Constants;
using Microsoft.AspNetCore.Authorization;

namespace WebApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IHubContext<AppHub> _appHubContext;
    private readonly IRoomRepository _roomRepository;
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _tokenService;
    private readonly IVideoPlayerStateService _videoPlayerStateService;
    private readonly IMapper _mapper;

    public UserController(
        IHubContext<AppHub> appHubContext,
        IRoomRepository roomRepository,
        IUserRepository userRepository,
        IJwtTokenService tokenService,
        IVideoPlayerStateService videoPlayerStateService,
        IMapper mapper)
    {
        _appHubContext = appHubContext;
        _roomRepository = roomRepository;
        _userRepository = userRepository;
        _tokenService = tokenService;
        _videoPlayerStateService = videoPlayerStateService;
        _mapper = mapper;
    }

    [HttpPost("Join/{roomHash}")]
    public async Task<IActionResult> JoinRoom([FromBody] RoomJoinInput input, [FromRoute] string roomHash, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || string.IsNullOrEmpty(roomHash))
        {
            return BadRequest();
        }

        var room = await _roomRepository.GetRoomAsync(roomHash, cancellationToken);

        if (room is null)
        {
            return NotFound();
        }

        if (room.RoomSettings.RoomPassword != input.RoomPassword)
        {
            return Unauthorized();
        }

        if (room.RoomSettings.MaxUsers == room.Users.Count)
        {
            return Forbid();
        }

        if (room.Users.Any(u => u.Username == input.Username))
        {
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
        var isNewUserAdded = await _userRepository.AddUserAsync(roomHash, newUser, cancellationToken);

        if (!isNewUserAdded)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        var output = new RoomJoinOutput
        {
            AuthorizationToken = jwt,
            IsAdmin = userRole == Role.Admin,
            ChatMessages = room.ChatMessages.ToList(),
            PlaylistVideos = room.PlaylistVideos.ToList(),
            Users = (await _userRepository.GetUsersDTOAsync(roomHash, cancellationToken)).ToList(),
            RoomSettings = room.RoomSettings,
            UserPermissions = room.UserPermissions,
            VideoPlayer = _videoPlayerStateService.GetVideoPlayer(roomHash) ?? new Core.Entities.In_memory.VideoPlayer()
        };

        await _appHubContext.Clients.Group(roomHash).SendAsync(HubMessages.OnJoinRoom, newUserDTO);

        var rooms = await _roomRepository.GetRoomsDTOAsync(cancellationToken);

        return Ok(output);
    }

    [Authorize]
    [HttpDelete("Leave/{roomHash}")]
    public async Task<IActionResult> LeaveRoom([FromRoute] string roomHash, CancellationToken cancellationToken)
    {
        var userIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userIdentifier is null)
        {
            return Unauthorized();
        }

        if (string.IsNullOrEmpty(roomHash))
        {
            return BadRequest();
        }

        var room = await _roomRepository.GetRoomAsync(roomHash, cancellationToken);

        if (room is null)
        {
            return NotFound();
        }

        var user = await _userRepository.GetUserAsync(roomHash, userIdentifier, cancellationToken);

        if (user is null)
        {
            return Unauthorized();
        }

        var deletedUser = await _userRepository.DeleteUserByUsernameAsync(roomHash, userIdentifier, cancellationToken);

        if (deletedUser is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        var updatedRoom = await _roomRepository.GetRoomAsync(roomHash, cancellationToken);

        if (updatedRoom?.Users.Count == 0)
        {
            await _roomRepository.DeleteRoomAsync(roomHash, cancellationToken);
        }

        var userDTO = _mapper.Map<UserDTO>(user);
        await _appHubContext.Clients.Group(roomHash).SendAsync(HubMessages.OnLeaveRoom, userDTO);

        return Ok();
    }
}
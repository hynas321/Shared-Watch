using Microsoft.AspNetCore.Mvc;
using DotnetServer.Infrastructure.Repositories;
using DotnetServer.Api.Handlers;
using DotnetServer.Api.HttpClasses.Input;
using DotnetServer.Core.Entities;
using DotnetServer.Api.HttpClasses.Output;
using DotnetServer.Core.Enums;
using DotnetServer.Api.DTO;
using DotnetServer.Shared.Constants;
using AutoMapper;
using DotnetServer.Shared.Helpers;

namespace DotnetServer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomController : ControllerBase
{
    private readonly ILogger<RoomController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IRoomRepository _roomRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRoomControllerHandler _roomHandler;
    private readonly IMapper _mapper;

    public RoomController(
        ILogger<RoomController> logger,
        IConfiguration configuration,
        IRoomRepository roomRepository,
        IUserRepository userRepository,
        IRoomControllerHandler roomHandler,
        IMapper mapper)
    {
        _logger = logger;
        _configuration = configuration;
        _roomRepository = roomRepository;
        _userRepository = userRepository;
        _roomHandler = roomHandler;
        _mapper = mapper;
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create(
        [FromBody] RoomCreateInput input,
        [FromHeader(Name = "X-SignalR-ConnectionId")] string signalRConnectionId = null,
        [FromHeader(Name = "Authorization")] string authorizationToken = null)
    {
        if (!ModelState.IsValid || string.IsNullOrEmpty(signalRConnectionId))
        {
            _logger.LogInformation($"Create: BadRequest. RoomName: {input.RoomName}, RoomPassword: {input.RoomPassword}, Username: {input.Username}");
            return BadRequest();
        }

        var existingUser = await _userRepository.GetUserByAuthorizationTokenAsync(authorizationToken);
        if (existingUser != null)
        {
            _logger.LogInformation($"Create: Unauthorized. RoomName: {input.RoomName}, RoomPassword: {input.RoomPassword}, Username: {input.Username}");
            return Unauthorized();
        }

        Room room = await _roomHandler.CreateRoom(input);

        if (room == null)
        {
            string roomPasswordInfo = string.IsNullOrEmpty(input.RoomPassword) ? "<No password>" : input.RoomPassword;
            _logger.LogInformation($"Create: Conflict. RoomName: {input.RoomName}, RoomPassword: {roomPasswordInfo}, Username: {input.Username}");
            return Conflict();
        }

        var output = new RoomCreateOutput
        {
            RoomHash = room.Hash
        };

        _logger.LogInformation($"Create: Created. RoomName: {input.RoomName}, RoomPassword: {input.RoomPassword}, Username: {input.Username}");

        var serializedOutput = JsonHelper.Serialize(output);

        return CreatedAtAction(nameof(Get), new { roomHash = room.Hash }, serializedOutput);
    }

    [HttpGet("Exists/{roomHash}")]
    public async Task<IActionResult> Exists([FromRoute] string roomHash)
    {
        if (string.IsNullOrEmpty(roomHash))
        {
            return BadRequest();
        }

        Room room = await _roomRepository.GetRoomAsync(roomHash);

        if (room == null)
        {
            return NotFound();
        }

        var output = new RoomExistsOutput
        {
            RoomType = string.IsNullOrEmpty(room.RoomSettings.RoomPassword) ? RoomTypes.Public : RoomTypes.Private
        };

        var serializedOutput = JsonHelper.Serialize(output);

        return Ok(serializedOutput);
    }

    [HttpGet("Get/{roomHash}")]
    public async Task<IActionResult> Get([FromRoute] string roomHash)
    {
        Room room = await _roomRepository.GetRoomAsync(roomHash);

        if (room == null)
        {
            _logger.LogInformation($"{roomHash} Get: NotFound.");
            return NotFound();
        }

        RoomDTO roomDTO = _mapper.Map<RoomDTO>(room);

        _logger.LogInformation($"GetAll: OK.");

        var serializedRoomsDTO = JsonHelper.Serialize(roomDTO);

        return Ok(serializedRoomsDTO);
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll()
    {
        IEnumerable<RoomDTO> roomsDTO = await _roomRepository.GetRoomsDTOAsync();

        _logger.LogInformation($"GetAll: OK.");

        var serializedRoomsDTO = JsonHelper.Serialize(roomsDTO);

        return Ok(serializedRoomsDTO);
    }

    [HttpGet("GetAllDetails")]
    public async Task<IActionResult> GetAllDetails([FromHeader] string globalAdminToken)
    {
        if (globalAdminToken != _configuration[AppSettingsVariables.GlobalAdminToken])
        {
            _logger.LogInformation("GetAllDetails: Unauthorized");
            return Unauthorized();
        }

        List<Room> rooms = await _roomRepository.GetRoomsAsync();

        _logger.LogInformation("GetAllDetails: OK");

        var serializedRooms = JsonHelper.Serialize(rooms);

        return Ok(serializedRooms);
    }
}
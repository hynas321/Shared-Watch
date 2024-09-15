using Microsoft.AspNetCore.Mvc;
using WebApi.Infrastructure.Repositories;
using WebApi.Api.Handlers;
using WebApi.Api.HttpClasses.Input;
using WebApi.Api.HttpClasses.Output;
using WebApi.Core.Enums;
using WebApi.Api.DTO;
using WebApi.Shared.Constants;
using AutoMapper;
using WebApi.Shared.Helpers;

namespace WebApi.Api.Controllers;

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
    public async Task<IActionResult> Create([FromBody] RoomCreateInput input)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogInformation($"Create: BadRequest. RoomName: {input.RoomName}, RoomPassword: {input.RoomPassword}, Username: {input.Username}");
            return BadRequest();
        }

        //User in any other room - add the check

        var room = await _roomHandler.CreateRoom(input);

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

        var room = await _roomRepository.GetRoomAsync(roomHash);

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
        var room = await _roomRepository.GetRoomAsync(roomHash);

        if (room == null)
        {
            _logger.LogInformation($"{roomHash} Get: NotFound.");
            return NotFound();
        }

        var roomDTO = _mapper.Map<RoomDTO>(room);

        _logger.LogInformation($"GetAll: OK.");

        var serializedRoomsDTO = JsonHelper.Serialize(roomDTO);

        return Ok(serializedRoomsDTO);
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll()
    {
        var roomsDTO = await _roomRepository.GetRoomsDTOAsync();

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

        var rooms = await _roomRepository.GetRoomsAsync();

        _logger.LogInformation("GetAllDetails: OK");

        var serializedRooms = JsonHelper.Serialize(rooms);

        return Ok(serializedRooms);
    }
}
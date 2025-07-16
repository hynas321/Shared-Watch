using Microsoft.AspNetCore.Mvc;
using WebApi.Infrastructure.Repositories;
using WebApi.Api.HttpClasses.Input;
using WebApi.Api.HttpClasses.Output;
using WebApi.Core.Enums;
using WebApi.Api.DTO;
using WebApi.Shared.Constants;
using AutoMapper;
using WebApi.Shared.Helpers;
using WebApi.Core.Entities;

namespace WebApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IRoomRepository _roomRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public RoomController(
        IConfiguration configuration,
        IRoomRepository roomRepository,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _configuration = configuration;
        _roomRepository = roomRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] RoomCreateInput input, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        if (await _roomRepository.GetRoomByNameAsync(input.RoomName, cancellationToken) != null)
        {
            return Conflict();
        }

        var room = new Room(input.RoomName, input.RoomPassword);

        await _roomRepository.AddRoomAsync(room, cancellationToken);

        var output = new RoomCreateOutput { RoomHash = room.Hash };
        var serializedOutput = JsonHelper.Serialize(output);

        return CreatedAtAction(nameof(Get), new { roomHash = room.Hash }, serializedOutput);
    }

    [HttpGet("Exists/{roomHash}")]
    public async Task<IActionResult> Exists([FromRoute] string roomHash, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(roomHash))
        {
            return BadRequest();
        }

        var room = await _roomRepository.GetRoomAsync(roomHash, cancellationToken);

        if (room is null)
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
    public async Task<IActionResult> Get([FromRoute] string roomHash, CancellationToken cancellationToken)
    {
        var room = await _roomRepository.GetRoomAsync(roomHash, cancellationToken);

        if (room is null)
        {
            return NotFound();
        }

        var roomDTO = _mapper.Map<RoomDTO>(room);
        var serializedRoomsDTO = JsonHelper.Serialize(roomDTO);

        return Ok(serializedRoomsDTO);
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var roomsDTO = await _roomRepository.GetRoomsDTOAsync(cancellationToken);
        var serializedRoomsDTO = JsonHelper.Serialize(roomsDTO);

        return Ok(serializedRoomsDTO);
    }

    [HttpGet("GetAllDetails")]
    public async Task<IActionResult> GetAllDetails([FromHeader] string globalAdminToken, CancellationToken cancellationToken)
    {
        if (globalAdminToken != _configuration[AppSettingsVariables.GlobalAdminToken])
        {
            return Unauthorized();
        }

        var rooms = await _roomRepository.GetRoomsAsync(cancellationToken);
        var serializedRooms = JsonHelper.Serialize(rooms);

        return Ok(serializedRooms);
    }
}
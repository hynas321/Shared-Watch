using Microsoft.AspNetCore.Mvc;
using WebApi.Application.Repositories;
using WebApi.Core.Enums;
using WebApi.Application.DTO;
using WebApi.Application.Services;
using WebApi.Application.Options;
using Microsoft.Extensions.Options;
using WebApi.Api.Contracts.Input;
using WebApi.Api.Contracts.Output;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomController : ControllerBase
{
    private readonly GlobalAdminOptions _globalAdminOptions;
    private readonly IRoomRepository _roomRepository;
    private readonly RoomApplicationService _roomApplicationService;
    private readonly ILogger<RoomController> _logger;
    public RoomController(
        IOptions<GlobalAdminOptions> globalAdminOptions,
        IRoomRepository roomRepository,
        RoomApplicationService roomApplicationService,
        ILogger<RoomController> logger)
    {
        _globalAdminOptions = globalAdminOptions.Value;
        _roomRepository = roomRepository;
        _roomApplicationService = roomApplicationService;
        _logger = logger;
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] RoomCreateInput input, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating room {RoomName}. PasswordProtected: {PasswordProtected}, RequestId: {RequestId}",
            input.RoomName,
            !string.IsNullOrEmpty(input.RoomPassword),
            HttpContext.TraceIdentifier);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Room creation validation failed for {RoomName}. RequestId: {RequestId}", input.RoomName, HttpContext.TraceIdentifier);
            return BadRequest();
        }

        var room = await _roomApplicationService.CreateRoomAsync(
            input.RoomName!, input.RoomPassword!, cancellationToken);
        if (room is null)
        {
            _logger.LogWarning("Room creation rejected because the room name already exists: {RoomName}. RequestId: {RequestId}", input.RoomName, HttpContext.TraceIdentifier);
            return Conflict();
        }

        _logger.LogInformation("Room {RoomHash} created with name {RoomName}. RequestId: {RequestId}", room.Hash, input.RoomName, HttpContext.TraceIdentifier);

        var output = new RoomCreateOutput { RoomHash = room.Hash };

        return CreatedAtAction(nameof(Get), new { roomHash = room.Hash }, output);
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
            _logger.LogDebug("Room lookup returned no room for hash {RoomHash}. RequestId: {RequestId}", roomHash, HttpContext.TraceIdentifier);
            return NotFound();
        }

        var output = new RoomExistsOutput
        {
            RoomType = string.IsNullOrEmpty(room.RoomSettings.RoomPassword) ? RoomTypes.Public : RoomTypes.Private
        };

        return Ok(output);
    }

    [HttpGet("Get/{roomHash}")]
    public async Task<IActionResult> Get([FromRoute] string roomHash, CancellationToken cancellationToken)
    {
        var room = await _roomRepository.GetRoomAsync(roomHash, cancellationToken);

        if (room is null)
        {
            return NotFound();
        }

        var roomDTO = RoomDTO.FromEntity(room);

        return Ok(roomDTO);
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var roomsDTO = await _roomRepository.GetRoomsDTOAsync(cancellationToken);

        return Ok(roomsDTO);
    }

    [HttpGet("Health")]
    public async Task<IActionResult> Health(CancellationToken cancellationToken)
    {
        await _roomRepository.GetRoomsDTOAsync(cancellationToken);
        return Ok();
    }

    [HttpGet("GetAllDetails")]
    public async Task<IActionResult> GetAllDetails([FromHeader] string? globalAdminToken, CancellationToken cancellationToken)
    {
        if (!IsGlobalAdminTokenValid(globalAdminToken))
        {
            return Unauthorized();
        }

        var rooms = await _roomRepository.GetRoomsAsync(cancellationToken);
        var safeRoomDetails = rooms.Select(room => new
        {
            RoomHash = room.Hash,
            RoomName = room.RoomSettings.RoomName,
            RoomType = room.RoomSettings.RoomType,
            OccupiedSlots = room.Users.Count,
            TotalSlots = room.RoomSettings.MaxUsers,
            Users = room.Users.Select(UserDTO.FromEntity),
            room.ChatMessages,
            room.PlaylistVideos,
            room.UserPermissions
        });

        return Ok(safeRoomDetails);
    }

    [HttpDelete("Delete/{roomHash}")]
    public async Task<IActionResult> Delete(
        [FromRoute] string roomHash,
        [FromHeader] string? globalAdminToken,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(roomHash))
        {
            return BadRequest();
        }

        if (!IsGlobalAdminTokenValid(globalAdminToken))
        {
            return Unauthorized();
        }

        var deletedRoom = await _roomRepository.DeleteRoomAsync(roomHash, cancellationToken);
        return deletedRoom is null ? NotFound() : NoContent();
    }

    private bool IsGlobalAdminTokenValid(string? suppliedToken)
    {
        var expectedToken = _globalAdminOptions.Token;
        return !string.IsNullOrWhiteSpace(expectedToken)
            && !string.IsNullOrWhiteSpace(suppliedToken)
            && System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
                System.Text.Encoding.UTF8.GetBytes(suppliedToken),
                System.Text.Encoding.UTF8.GetBytes(expectedToken));
    }
}


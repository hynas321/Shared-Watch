using Dotnet.Server.Configuration;
using Dotnet.Server.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace dotnet_server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomController : ControllerBase
{
    private readonly ILogger<RoomController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IRoomRepository _roomRepository;
    private readonly IUserRepository _userRepository;

    public RoomController(
        ILogger<RoomController> logger,
        IConfiguration configuration,
        IRoomRepository roomRepository,
        IUserRepository userRepository)
    {
        _logger = logger;
        _configuration = configuration;
        _roomRepository = roomRepository;
        _userRepository = userRepository;
    }

    [HttpPost("Create")]
    public IActionResult Create(
        [FromBody] RoomCreateInput input,
        [FromHeader(Name = "X-SignalR-ConnectionId")] string signalRConnectionId = null,
        [FromHeader(Name = "Authorization")] string authorizationToken = null)
    {
        try
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(signalRConnectionId))
            {
                _logger.LogInformation(
                    $"Create: Status 400. RoomName: {input.RoomName}, RoomPassword: {input.RoomPassword}, Username: {input.Username}"
                );

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            bool isUserConnectedInOtherRoom = _userRepository.GetUserByAuthorizationToken(authorizationToken) != null;

            if (isUserConnectedInOtherRoom)
            {
                _logger.LogInformation(
                    $"Create: Status 401. RoomName: {input.RoomName}, RoomPassword: {input.RoomPassword}, Username: {input.Username}"
                );

                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            RoomTypes roomType = input.RoomPassword.Length == 0 ? RoomTypes.Public : RoomTypes.Private;
            Room room = new Room(input.RoomName, input.RoomPassword, roomType);

            bool roomAdded = _roomRepository.AddRoom(room);

            if (!roomAdded)
            {
                string roomPasswordInfo = input.RoomPassword.Length > 0 ? input.RoomPassword : "<No password>";

                _logger.LogInformation(
                    $"Create: Status 409. RoomName: {input.RoomName}, RoomPassword: {roomPasswordInfo}, Username: {input.Username}"
                );

                return StatusCode(StatusCodes.Status409Conflict);
            }

            RoomCreateOutput output = new RoomCreateOutput()
            {
                RoomHash = room.RoomHash
            };

            _logger.LogInformation(
                $"Create: Status 201. RoomName: {input.RoomName}, RoomPassword: {input.RoomPassword}, Username: {input.Username}"
            );

            return StatusCode(StatusCodes.Status201Created, JsonHelper.Serialize(output));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("Exists/{roomHash}")]
    public IActionResult Exists([FromRoute] string roomHash)
    {
        if (roomHash == "")
        {
            return StatusCode(StatusCodes.Status400BadRequest);
        }

        Room room = _roomRepository.GetRoom(roomHash);

        if (room == null)
        {
            return StatusCode(StatusCodes.Status404NotFound);
        }

        RoomExistsOutput output = new RoomExistsOutput()
        {
            RoomType = room.RoomSettings.RoomPassword == "" ? RoomTypes.Public : RoomTypes.Private
        };

        return StatusCode(StatusCodes.Status200OK, JsonHelper.Serialize(output));
    }

    [HttpGet("Get/{roomHash}")]
    public IActionResult Get([FromRoute] string roomHash)
    {
        try
        {
            Room room = _roomRepository.GetRoom(roomHash);
            
            if (room == null)
            {
                _logger.LogInformation($"{roomHash} Get: Status 404.");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            RoomDTO roomDTO = new RoomDTO(
                room.RoomHash,
                room.RoomSettings.RoomName,
                room.RoomSettings.RoomType,
                room.Users.Count(),
                room.RoomSettings.MaxUsers
            );

            _logger.LogInformation($"GetAll: Status 200.");

            return StatusCode(StatusCodes.Status200OK, JsonHelper.Serialize(roomDTO));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("GetAll")]
    public IActionResult GetAll()
    {
        try
        {
            IEnumerable<RoomDTO> roomsDTO = _roomRepository.GetRoomsDTO();

            _logger.LogInformation($"GetAll: Status 200.");

            return StatusCode(StatusCodes.Status200OK, JsonHelper.Serialize(roomsDTO));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("GetAllDetails")]
    public IActionResult GetAllDetails([FromHeader] string globalAdminToken)
    {
        try
        {
            if (globalAdminToken != _configuration[AppSettingsVariables.GlobalAdminToken])
            {
                _logger.LogInformation("GetAllDetails: Status 401");

                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            List<Room> rooms = _roomRepository.GetRooms();

            _logger.LogInformation("GetAllDetails: Status 200");

            return StatusCode(StatusCodes.Status200OK, JsonHelper.Serialize(rooms));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
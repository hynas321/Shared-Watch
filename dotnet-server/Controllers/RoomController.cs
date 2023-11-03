using Dotnet.Server.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomController : ControllerBase
{
    private readonly ILogger<RoomController> _logger;
    private readonly IConfiguration _configuration;
    private readonly RoomManager _roomManager = new RoomManager();

    public RoomController(ILogger<RoomController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    [HttpGet("GetAll")]
    public IActionResult GetAll()
    {
        try 
        {   
            IEnumerable<RoomDTO> roomsDTO = _roomManager.GetAllRoomsDTO();

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
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            IEnumerable<Room> rooms = _roomManager.GetAllRooms();

            return StatusCode(StatusCodes.Status200OK, JsonHelper.Serialize(rooms));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost("Create")]
    public IActionResult Create([FromBody] RoomCreateInput input)
    {
        try
        {
            if (!ModelState.IsValid)
            {   
                return StatusCode(StatusCodes.Status400BadRequest);
            }
    
            Room room = new Room(input.RoomName, input.RoomPassword);

            bool roomAdded = _roomManager.AddRoom(room);

            if (!roomAdded)
            {
                return StatusCode(StatusCodes.Status409Conflict);
            }
            
            User user = new User(input.Username, true);

            bool userAdded = _roomManager.AddUser(room.RoomHash, user);

            if (!userAdded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            RoomCreateOutput output = new RoomCreateOutput()
            {
                RoomHash = room.RoomHash,
                AccessToken = user.AccessToken,
            };

            return StatusCode(StatusCodes.Status201Created, JsonHelper.Serialize(output));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
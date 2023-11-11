using Dotnet.Server.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

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
            
            User user = new User(
                username: input.Username,
                isAdmin: true,
                isInRoom: false
            );

            bool isUserAdded = _roomManager.AddUser(room.RoomHash, user);

            if (!isUserAdded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            RoomCreateOutput output = new RoomCreateOutput()
            {
                RoomHash = room.RoomHash,
                AccessToken = user.AuthorizationToken,
            };

            return StatusCode(StatusCodes.Status201Created, JsonHelper.Serialize(output));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost("Join/{roomHash}")]
    public IActionResult Join([FromBody] RoomJoinInput input, [FromRoute] string roomHash)
    {
        try
        {
            string authorizationToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");

            if (!ModelState.IsValid || roomHash == "")
            {   
                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Room room = _roomManager.GetRoom(roomHash);

            if (room == null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }

            if (room.RoomPassword != input.RoomPassword)
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            if (room.Users.Any(u => u.Username == input.Username && u.IsInRoom == true))
            {   
                return StatusCode(StatusCodes.Status409Conflict);
            }

            User user = _roomManager.GetUser(roomHash, authorizationToken);
            RoomJoinOutput output = new RoomJoinOutput();

            /*
            if (user != null && authorizationToken != user.AuthorizationToken)
            {
                return StatusCode(StatusCodes.Status409Conflict);
            }

            if (user != null && authorizationToken == user.AuthorizationToken && user.IsInRoom == true)
            {
                return StatusCode(StatusCodes.Status409Conflict);
            }

            if (user != null && authorizationToken == user.AuthorizationToken && user.IsInRoom == false)
            {
                output.AccessToken = authorizationToken;
                user.IsInRoom = true;
                return StatusCode(StatusCodes.Status200OK, JsonHelper.Serialize(output));
            }*/

            User newUser = new User(
                input.Username,
                isInRoom: true,
                isAdmin: false
            );

            output.AccessToken = newUser.AuthorizationToken;
            return StatusCode(StatusCodes.Status200OK, JsonHelper.Serialize(output));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpDelete("Leave/{roomHash}")]
    public IActionResult Leave([FromRoute] string roomHash)
    {
        try
        {
            string authorizationToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");

            if (roomHash == "" || authorizationToken == "")
            {   
                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Room room = _roomManager.GetRoom(roomHash);

            if (room == null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }

            User user = _roomManager.GetUser(roomHash, authorizationToken);

            if (user == null)
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            user.IsInRoom = false;

            //Remove room if no user is inside
            if (room.Users.Count(u => u.IsInRoom == true) == 0)
            {
                _roomManager.RemoveRoom(roomHash);
            }

            return StatusCode(StatusCodes.Status200OK);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
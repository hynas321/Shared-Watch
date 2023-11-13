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

    [HttpPost("Create")]
    public IActionResult Create([FromBody] RoomCreateInput input)
    {
        try
        {
            if (!ModelState.IsValid)
            {   
                _logger.LogInformation("Create: Status 400");
                return StatusCode(StatusCodes.Status400BadRequest);
            }
    
            Room room = new Room(input.RoomName, input.RoomPassword);

            bool roomAdded = _roomManager.AddRoom(room);

            if (!roomAdded)
            {
                _logger.LogInformation("Create: Status 409");
                return StatusCode(StatusCodes.Status409Conflict);
            }

            RoomCreateOutput output = new RoomCreateOutput()
            {
                RoomHash = room.RoomHash
            };

            _logger.LogInformation("Create: Status 201");
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
                _logger.LogInformation("Join: Status 500");
                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Room room = _roomManager.GetRoom(roomHash);

            if (room == null)
            {
                _logger.LogInformation("Join: Status 404");
                return StatusCode(StatusCodes.Status404NotFound);
            }

            if (room.RoomPassword != input.RoomPassword)
            {
                _logger.LogInformation("Join: Status 401");
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            if (room.RoomSettings.MaxUsers == room.Users.Count(u => u.IsInRoom == true))
            {
                _logger.LogInformation("Join: Status 409");
                return StatusCode(StatusCodes.Status409Conflict);
            }

            if (room.Users.Any(u => u.Username == input.Username && u.IsInRoom == true && authorizationToken != u.AuthorizationToken))
            {
                _logger.LogInformation("Join: Status 409");
                return StatusCode(StatusCodes.Status409Conflict);
            }

            if (room.Users.Any(u => u.Username != input.Username && u.IsInRoom == true && authorizationToken == u.AuthorizationToken))
            {
                _logger.LogInformation("Join: Status 409");
                return StatusCode(StatusCodes.Status409Conflict);
            }

            if (room.Users.Any(u => u.Username == input.Username && u.IsInRoom == true && authorizationToken == u.AuthorizationToken))
            {
                _logger.LogInformation("Join: Status 409");
                return StatusCode(StatusCodes.Status409Conflict);
            }


            RoomJoinOutput output;
            bool isUserFirstAdmin = false;

            if (room.Users.Count == 0)
            {
                isUserFirstAdmin = true;
            }

            User newUser = new User(
                input.Username,
                isInRoom: true,
                isAdmin: isUserFirstAdmin
            );
            
            output = new RoomJoinOutput()
            {
                AuthorizationToken = newUser.AuthorizationToken,
                ChatMessages = room.ChatMessages,
                QueuedVideos = room.QueuedVideos,
                Users = _roomManager.GetUsersDTO(roomHash).ToList(),
                RoomSettings = room.RoomSettings,
                VideoPlayerSettings = room.VideoPlayerSettings

            };

            bool isNewUserAdded = _roomManager.AddUser(roomHash, newUser);

            if (!isNewUserAdded)
            {
                _logger.LogInformation("Join: Status 500");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            _logger.LogInformation("Join: Status 200");
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
                _logger.LogInformation($"Leave: Status 400. authorizationToken: {authorizationToken}, roomHash: {roomHash}");
                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Room room = _roomManager.GetRoom(roomHash);

            if (room == null)
            {
                _logger.LogInformation($"Leave: Status 404. authorizationToken: {authorizationToken}, roomHash: {roomHash}");
                return StatusCode(StatusCodes.Status404NotFound);
            }

            User user = _roomManager.GetUser(roomHash, authorizationToken);

            if (user == null)
            {
                _logger.LogInformation($"Leave: Status 401. authorizationToken: {authorizationToken}, roomHash: {roomHash}");
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            bool isUserInRoomUpdated = _roomManager.UpdateUserInRoom(roomHash, authorizationToken, false);

            if (!isUserInRoomUpdated)
            {
                _logger.LogInformation($"Leave: Status 500. authorizationToken: {authorizationToken}, roomHash: {roomHash}");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            Room updatedRoom = _roomManager.GetRoom(roomHash);
            //Remove room if no user is inside
            _logger.LogInformation(updatedRoom.Users.Count(u => u.IsInRoom == true).ToString());
            if (updatedRoom.Users.Count(u => u.IsInRoom == true) == 0)
            {
                _logger.LogInformation($"Room {roomHash} has been removed");
                _roomManager.RemoveRoom(roomHash);
            }

            _logger.LogInformation($"Leave: Status 200. authorizationToken: {authorizationToken}, roomHash: {roomHash}");
            return StatusCode(StatusCodes.Status200OK);
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
            IEnumerable<RoomDTO> roomsDTO = _roomManager.GetAllRoomsDTO();

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

            IEnumerable<Room> rooms = _roomManager.GetAllRooms();

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
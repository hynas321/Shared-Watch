using Dotnet.Server.Configuration;
using Dotnet.Server.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Net.Http.Headers;

namespace dotnet_server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomController : ControllerBase
{
    private readonly ILogger<RoomController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHubContext<RoomHub> _roomHubContext;
    private readonly RoomManager _roomManager = new RoomManager();

    public RoomController(
        ILogger<RoomController> logger,
        IConfiguration configuration,
        IHubContext<RoomHub> roomHubContext
    )
    {
        _logger = logger;
        _configuration = configuration;
        _roomHubContext = roomHubContext;
    }

    [HttpPost("Create")]
    public IActionResult Create([FromBody] RoomCreateInput input)
    {
        try
        {
            if (!ModelState.IsValid)
            {   
                _logger.LogInformation($"Create: Status 400. RoomName: {input.RoomName}, RoomPassword: {input.RoomPassword}, Username: {input.Username}");
                return StatusCode(StatusCodes.Status400BadRequest);
            }
    
            Room room = new Room(input.RoomName, input.RoomPassword);

            bool roomAdded = _roomManager.AddRoom(room);

            if (!roomAdded)
            {
                _logger.LogInformation($"Create: Status 409. RoomName: {input.RoomName}, RoomPassword: {input.RoomPassword}, Username: {input.Username}");
                return StatusCode(StatusCodes.Status409Conflict);
            }

            RoomCreateOutput output = new RoomCreateOutput()
            {
                RoomHash = room.RoomHash
            };

            _logger.LogInformation($"Create: Status 201. RoomName: {input.RoomName}, RoomPassword: {input.RoomPassword}, Username: {input.Username}");
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

        Room room = _roomManager.GetRoom(roomHash);

        if (room == null)
        {
            return StatusCode(StatusCodes.Status404NotFound);
        }

        RoomExistsOutput output = new RoomExistsOutput()
        {
            RoomType = room.RoomPassword == "" ? RoomTypesEnum.Public : RoomTypesEnum.Private
        };

        return StatusCode(StatusCodes.Status200OK, JsonHelper.Serialize(output));
    }

    [HttpPost("Join/{roomHash}")]
    public IActionResult Join([FromBody] RoomJoinInput input, [FromRoute] string roomHash)
    {
        try
        {
            string authorizationToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            _logger.LogInformation("Authorization token: " + authorizationToken);

            if (!ModelState.IsValid || roomHash == "")
            {   
                _logger.LogInformation($"{roomHash} Join: Status 500. RoomPassword: {input.RoomPassword}, Username: {input.Username}");
                return StatusCode(StatusCodes.Status400BadRequest);
            }

            _logger.LogInformation(authorizationToken);

            Room room = _roomManager.GetRoom(roomHash);

            if (room == null)
            {
                _logger.LogInformation($"{roomHash} Join: Status 404. RoomPassword: {input.RoomPassword}, Username: {input.Username}");
                return StatusCode(StatusCodes.Status404NotFound);
            }

            if (room.RoomPassword != input.RoomPassword)
            {
                _logger.LogInformation($"{roomHash} Join: Status 401. RoomPassword: {input.RoomPassword}, Username: {input.Username}");
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            if (room.RoomSettings.MaxUsers == room.Users.Count)
            {
                _logger.LogInformation($"{roomHash} Join: Status 409. RoomPassword: {input.RoomPassword}, Username: {input.Username}");
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            if (room.Users.Any(u => u.Username == input.Username))
            {
                _logger.LogInformation($"{roomHash} Join: Status 409. RoomPassword: {input.RoomPassword}, Username: {input.Username}");
                return StatusCode(StatusCodes.Status409Conflict);
            }

            bool isUserFirstAdmin = false;

            if (room.Users.Count == 0)
            {
                isUserFirstAdmin = true;
            }

            User newUser = new User(input.Username, isUserFirstAdmin);
            UserDTO newUserDTO = new UserDTO(newUser.Username, newUser.IsAdmin);

            bool isNewUserAdded = _roomManager.AddUser(roomHash, newUser);

            if (!isNewUserAdded)
            {
                _logger.LogInformation($"{roomHash} Join: Status 500. RoomPassword: {input.RoomPassword}, Username: {input.Username}");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            
            RoomJoinOutput output = new RoomJoinOutput()
            {
                AuthorizationToken = newUser.AuthorizationToken,
                ChatMessages = room.ChatMessages,
                QueuedVideos = room.QueuedVideos,
                Users = _roomManager.GetUsersDTO(roomHash).ToList(),
                RoomSettings = room.RoomSettings,
                VideoPlayerSettings = room.VideoPlayerSettings
            };
            
            var hubContext = _roomHubContext.Groups.AddToGroupAsync(HttpContext.Connection.Id, roomHash);

            _roomHubContext.Clients.All.SendAsync(HubEvents.OnJoinRoom, newUserDTO);

            _logger.LogInformation($"{roomHash} Join: Status 200. RoomPassword: {input.RoomPassword}, Username: {input.Username}");
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
                _logger.LogInformation($"{roomHash} Leave: Status 400. AuthorizationToken: {authorizationToken}");
                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Room room = _roomManager.GetRoom(roomHash);

            if (room == null)
            {
                _logger.LogInformation($"{roomHash} Leave: Status 404. AuthorizationToken: {authorizationToken}");
                return StatusCode(StatusCodes.Status404NotFound);
            }

            User user = _roomManager.GetUser(roomHash, authorizationToken);

            if (user == null)
            {
                _logger.LogInformation($"{roomHash} Leave: Status 401. AuthorizationToken: {authorizationToken}");
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            User deletedUser = _roomManager.DeleteUser(roomHash, authorizationToken);

            if (deletedUser == null)
            {
                _logger.LogInformation($"{roomHash} Leave: Status 500. AuthorizationToken: {authorizationToken}");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            Room updatedRoom = _roomManager.GetRoom(roomHash);

            if (updatedRoom.Users.Count == 0)
            {
                _logger.LogInformation($"Room {roomHash} has been removed");
                _roomManager.DeleteRoom(roomHash);
            }

            UserDTO userDTO = new UserDTO(user.Username, user.IsAdmin);

            var hubContext = _roomHubContext.Groups.AddToGroupAsync(HttpContext.Connection.Id, roomHash);

            _roomHubContext.Clients.All.SendAsync(HubEvents.OnLeaveRoom, userDTO);

            _logger.LogInformation($"{roomHash} Leave: Status 200. AuthorizationToken: {authorizationToken}");
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
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
    private readonly UserManager _userManager = new UserManager();

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
            string authorizationToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            string signalRConnectionId = Request.Headers["X-SignalR-ConnectionId"];

            if (!ModelState.IsValid || signalRConnectionId == null)
            {   
                _logger.LogInformation(
                    $"Create: Status 400. RoomName: {input.RoomName}, RoomPassword: {input.RoomPassword}, Username: {input.Username}"
                );

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            bool isUserConnectedInOtherRoom = _userManager.GetUserByAuthorizationToken(authorizationToken) != null;

            if (isUserConnectedInOtherRoom)
            {
                _logger.LogInformation(
                    $"Create: Status 401. RoomName: {input.RoomName}, RoomPassword: {input.RoomPassword}, Username: {input.Username}"
                );

                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            RoomTypesEnum roomType= input.RoomPassword.Length == 0 ? RoomTypesEnum.Public : RoomTypesEnum.Private;
            Room room = new Room(input.RoomName, input.RoomPassword, roomType);

            bool roomAdded = _roomManager.AddRoom(room);

            if (!roomAdded)
            {
                _logger.LogInformation(
                    $"Create: Status 409. RoomName: {input.RoomName}, RoomPassword: {input.RoomPassword}, Username: {input.Username}"
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

        Room room = _roomManager.GetRoom(roomHash);

        if (room == null)
        {
            return StatusCode(StatusCodes.Status404NotFound);
        }

        RoomExistsOutput output = new RoomExistsOutput()
        {
            RoomType = room.RoomSettings.RoomPassword == "" ? RoomTypesEnum.Public : RoomTypesEnum.Private
        };

        return StatusCode(StatusCodes.Status200OK, JsonHelper.Serialize(output));
    }

    [HttpPost("JoinRoom/{roomHash}")]
    public IActionResult JoinRoom([FromBody] RoomJoinInput input, [FromRoute] string roomHash)
    {
        try
        {
            string authorizationToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            string signalRConnectionId = Request.Headers["X-SignalR-ConnectionId"];

            if (!ModelState.IsValid || roomHash == "" || signalRConnectionId == null)
            {   
                _logger.LogInformation(
                    $"{roomHash} Join: Status 500. RoomPassword: {input.RoomPassword}, Username: {input.Username}"
                );
    
                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Room room = _roomManager.GetRoom(roomHash);

            if (room == null)
            {
                _logger.LogInformation(
                    $"{roomHash} Join: Status 404. RoomPassword: {input.RoomPassword}, Username: {input.Username}"
                );

                return StatusCode(StatusCodes.Status404NotFound);
            }

            if (room.RoomSettings.RoomPassword != input.RoomPassword)
            {
                _logger.LogInformation(
                    $"{roomHash} Join: Status 401. RoomPassword: {input.RoomPassword}, Username: {input.Username}"
                );

                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            if (room.RoomSettings.MaxUsers == room.Users.Count)
            {
                _logger.LogInformation(
                    $"{roomHash} Join: Status 403. RoomPassword: {input.RoomPassword}, Username: {input.Username}"
                );

                return StatusCode(StatusCodes.Status403Forbidden);
            }

            if (room.Users.Any(u => u.Username == input.Username))
            {
                _logger.LogInformation(
                    $"{roomHash} Join: Status 409. RoomPassword: {input.RoomPassword}, Username: {input.Username}"
                );

                return StatusCode(StatusCodes.Status409Conflict);
            }

            if (room.Users.Any(u => u.AuthorizationToken == authorizationToken))
            {
                _logger.LogInformation(
                    $"{roomHash} Join: Status 409. RoomPassword: {input.RoomPassword}, Username: {input.Username}"
                );

                return StatusCode(StatusCodes.Status409Conflict);
            }

            bool isUserAdmin = false;
            User newUser;

            if (room.Users.Count == 0)
            {
                isUserAdmin = true;
            }

            if (room.AdminTokens.Any(o => o == authorizationToken))
            {
                isUserAdmin = true;
                newUser = new User(input.Username, isUserAdmin, authorizationToken, signalRConnectionId);
            }
            else
            {
                newUser = new User(input.Username, isUserAdmin);
            }

            UserDTO newUserDTO = new UserDTO(newUser.Username, newUser.IsAdmin);

            if (room.Users.Count == 0)
            {
                room.AdminTokens.Add(newUser.AuthorizationToken);
            }

            var hubContext = _roomHubContext.Groups.AddToGroupAsync(signalRConnectionId, roomHash);

            bool isNewUserAdded = _userManager.AddUser(roomHash, newUser);

            if (!isNewUserAdded)
            {
                _logger.LogInformation(
                    $"{roomHash} Join: Status 500. RoomPassword: {input.RoomPassword}, Username: {input.Username}"
                );

                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            RoomJoinOutput output = new RoomJoinOutput()
            {
                AuthorizationToken = newUser.AuthorizationToken,
                IsAdmin = newUser.IsAdmin,
                ChatMessages = room.ChatMessages,
                PlaylistVideos = room.PlaylistVideos,
                Users = _userManager.GetUsersDTO(roomHash).ToList(),
                RoomSettings = room.RoomSettings,
                UserPermissions = room.UserPermissions,
                VideoPlayerState = room.VideoPlayerState
            };

            _roomHubContext.Clients.Group(roomHash).SendAsync(HubEvents.OnJoinRoom, newUserDTO);

            IEnumerable<RoomDTO> rooms = _roomManager.GetRoomsDTO();

            _roomHubContext.Clients.All.SendAsync(HubEvents.OnListOfRoomsUpdated, JsonHelper.Serialize(rooms));

            _logger.LogInformation(
                $"{roomHash} Join: Status 200. RoomPassword: {input.RoomPassword}, Username: {input.Username}"
            );

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
            string signalRConnectionId = Request.Headers["X-SignalR-ConnectionId"];

            if (roomHash == "" || authorizationToken == "" || signalRConnectionId == null)
            {
                _logger.LogInformation(
                    $"{roomHash} Leave: Status 400. AuthorizationToken: {authorizationToken}"
                );

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            Room room = _roomManager.GetRoom(roomHash);

            if (room == null)
            {
                _logger.LogInformation(
                    $"{roomHash} Leave: Status 404. AuthorizationToken: {authorizationToken}"
                );

                return StatusCode(StatusCodes.Status404NotFound);
            }

            User user = _userManager.GetUserByAuthorizationToken(roomHash, authorizationToken);

            if (user == null)
            {
                _logger.LogInformation(
                    $"{roomHash} Leave: Status 401. AuthorizationToken: {authorizationToken}"
                );

                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            User deletedUser = _userManager.DeleteUser(roomHash, authorizationToken);

            if (deletedUser == null)
            {
                _logger.LogInformation(
                    $"{roomHash} Leave: Status 500. AuthorizationToken: {authorizationToken}"
                );

                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            Room updatedRoom = _roomManager.GetRoom(roomHash);

            if (updatedRoom.Users.Count == 0)
            {
                _logger.LogInformation(
                    $"Room {roomHash} has been deleted"
                );

                _roomManager.DeleteRoom(roomHash);
            }

            UserDTO userDTO = new UserDTO(user.Username, user.IsAdmin);

            var hubContext = _roomHubContext.Groups.AddToGroupAsync(signalRConnectionId, roomHash);

            _roomHubContext.Clients.Group(roomHash).SendAsync(HubEvents.OnLeaveRoom, userDTO);

            IEnumerable<RoomDTO> rooms = _roomManager.GetRoomsDTO();

            _roomHubContext.Clients.All.SendAsync(HubEvents.OnListOfRoomsUpdated, JsonHelper.Serialize(rooms));

            _logger.LogInformation(
                $"{roomHash} Leave: Status 200. AuthorizationToken: {authorizationToken}"
            );

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
            IEnumerable<RoomDTO> roomsDTO = _roomManager.GetRoomsDTO();

            _logger.LogInformation($"GetAll: Status 200.");

            return StatusCode(StatusCodes.Status200OK, JsonHelper.Serialize(roomsDTO));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("Get/{roomHash}")]
    public IActionResult Get([FromRoute] string roomHash)
    {
        try
        {
            Room room = _roomManager.GetRoom(roomHash);
            
            if (room == null)
            {
                _logger.LogInformation($"{roomHash} Get: Status 404.");

                return StatusCode(StatusCodes.Status404NotFound);
            }

            RoomDTO roomDTO = new RoomDTO(
                room.RoomHash,
                room.RoomSettings.RoomName,
                room.RoomSettings.RoomType,
                room.Users.Count,
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

            IEnumerable<Room> rooms = _roomManager.GetRooms();

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
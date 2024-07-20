using Dotnet.Server.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace dotnet_server.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly IHubContext<AppHub> _appHubContext;
        private readonly IRoomRepository _roomRepository;
        private readonly IUserRepository _userRepository;

        public UserController(
            ILogger<UserController> logger,
            IHubContext<AppHub> appHubContext,
            IRoomRepository roomRepository,
            IUserRepository userRepository)
        {
            _logger = logger;
            _appHubContext = appHubContext;
            _roomRepository = roomRepository;
            _userRepository = userRepository;
        }

        [HttpPost("JoinRoom/{roomHash}")]
        public IActionResult JoinRoom(
            [FromBody] RoomJoinInput input,
            [FromRoute] string roomHash,
            [FromHeader(Name = "X-SignalR-ConnectionId")] string signalRConnectionId = null,
            [FromHeader(Name = "Authorization")] string authorizationToken = null)
        {
            try
            {
                if (!ModelState.IsValid || roomHash == "" || signalRConnectionId == null)
                {
                    _logger.LogInformation($"{roomHash} Join: Status 400. RoomPassword: {input.RoomPassword}, Username: {input.Username}");
                    return StatusCode(StatusCodes.Status400BadRequest);
                }

                Room room = _roomRepository.GetRoom(roomHash);

                if (room == null)
                {
                    _logger.LogInformation($"{roomHash} Join: Status 404. RoomPassword: {input.RoomPassword}, Username: {input.Username}");
                    return StatusCode(StatusCodes.Status404NotFound);
                }

                if (room.RoomSettings.RoomPassword != input.RoomPassword)
                {
                    _logger.LogInformation($"{roomHash} Join: Status 401. RoomPassword: {input.RoomPassword}, Username: {input.Username}");
                    return StatusCode(StatusCodes.Status401Unauthorized);
                }

                if (room.RoomSettings.MaxUsers == room.Users.Count())
                {
                    _logger.LogInformation($"{roomHash} Join: Status 403. RoomPassword: {input.RoomPassword}, Username: {input.Username}");
                    return StatusCode(StatusCodes.Status403Forbidden);
                }

                if (room.Users.Any(u => u.Username == input.Username))
                {
                    _logger.LogInformation($"{roomHash} Join: Status 409. RoomPassword: {input.RoomPassword}, Username: {input.Username}");
                    return StatusCode(StatusCodes.Status409Conflict);
                }

                if (room.Users.Any(u => u.AuthorizationToken == authorizationToken))
                {
                    _logger.LogInformation($"{roomHash} Join: Status 409. RoomPassword: {input.RoomPassword}, Username: {input.Username}");

                    return StatusCode(StatusCodes.Status409Conflict);
                }

                bool isUserAdmin = false;
                User newUser;

                if (room.Users.Count() == 0) isUserAdmin = true;

                if (room.AdminTokens.Any(o => o == authorizationToken))
                {
                    isUserAdmin = true;
                    newUser = new User(input.Username, isUserAdmin, authorizationToken, signalRConnectionId);
                }
                else
                {
                    newUser = new User(input.Username, isUserAdmin, signalRConnectionId: signalRConnectionId);
                }

                UserDTO newUserDTO = new UserDTO(newUser.Username, newUser.IsAdmin);

                if (room.Users.Count == 0) room.AdminTokens.Add(newUser.AuthorizationToken);

                var hubContext = _appHubContext.Groups.AddToGroupAsync(signalRConnectionId, roomHash);

                bool isNewUserAdded = _userRepository.AddUser(roomHash, newUser);

                if (!isNewUserAdded)
                {
                    _logger.LogInformation($"{roomHash} Join: Status 500. RoomPassword: {input.RoomPassword}, Username: {input.Username}");
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }

                RoomJoinOutput output = new RoomJoinOutput()
                {
                    AuthorizationToken = newUser.AuthorizationToken,
                    IsAdmin = newUser.IsAdmin,
                    ChatMessages = room.ChatMessages,
                    PlaylistVideos = room.PlaylistVideos,
                    Users = _userRepository.GetUsersDTO(roomHash).ToList(),
                    RoomSettings = room.RoomSettings,
                    UserPermissions = room.UserPermissions,
                    VideoPlayer = room.VideoPlayer
                };

                _appHubContext.Clients.Group(roomHash).SendAsync(HubMessages.OnJoinRoom, newUserDTO);

                IEnumerable<RoomDTO> rooms = _roomRepository.GetRoomsDTO();

                _appHubContext.Clients.All.SendAsync(HubMessages.OnListOfRoomsUpdated, JsonHelper.Serialize(rooms));

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
        public IActionResult Leave(
            [FromRoute] string roomHash,
            [FromHeader(Name = "X-SignalR-ConnectionId")] string signalRConnectionId = null,
            [FromHeader(Name = "Authorization")] string authorizationToken = null)
        {
            try
            {
                if (roomHash == "" || authorizationToken == "" || signalRConnectionId == null)
                {
                    _logger.LogInformation($"{roomHash} Leave: Status 400. AuthorizationToken: {authorizationToken}");
                    return StatusCode(StatusCodes.Status400BadRequest);
                }

                Room room = _roomRepository.GetRoom(roomHash);

                if (room == null)
                {
                    _logger.LogInformation($"{roomHash} Leave: Status 404. AuthorizationToken: {authorizationToken}");
                    return StatusCode(StatusCodes.Status404NotFound);
                }

                User user = _userRepository.GetUserByAuthorizationToken(roomHash, authorizationToken);

                if (user == null)
                {
                    _logger.LogInformation($"{roomHash} Leave: Status 401. AuthorizationToken: {authorizationToken}");
                    return StatusCode(StatusCodes.Status401Unauthorized);
                }

                User deletedUser = _userRepository.DeleteUser(roomHash, authorizationToken);

                if (deletedUser == null)
                {
                    _logger.LogInformation($"{roomHash} Leave: Status 500. AuthorizationToken: {authorizationToken}");
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }

                Room updatedRoom = _roomRepository.GetRoom(roomHash);

                if (updatedRoom.Users.Count == 0)
                {
                    _logger.LogInformation($"Room {roomHash} has been deleted");
                    _roomRepository.DeleteRoom(roomHash);
                }

                _appHubContext.Groups.RemoveFromGroupAsync(signalRConnectionId, roomHash);

                UserDTO userDTO = new UserDTO(user.Username, user.IsAdmin);

                _appHubContext.Clients.Group(roomHash).SendAsync(HubMessages.OnLeaveRoom, userDTO);

                IEnumerable<RoomDTO> rooms = _roomRepository.GetRoomsDTO();

                _appHubContext.Clients.All.SendAsync(HubMessages.OnListOfRoomsUpdated, JsonHelper.Serialize(rooms));

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
    }
}

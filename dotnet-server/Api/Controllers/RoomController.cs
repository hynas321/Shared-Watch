using Microsoft.AspNetCore.Mvc;
using DotnetServer.Infrastructure.Repositories;
using DotnetServer.Api.Handlers;
using DotnetServer.Api.HttpClasses.Input;
using DotnetServer.Core.Entities;
using DotnetServer.Api.HttpClasses.Output;
using DotnetServer.Core.Enums;
using DotnetServer.Api.DTO;
using DotnetServer.Shared.Helpers;
using DotnetServer.Shared.Constants;
using AutoMapper;

namespace DotnetServer.Api.Controllers;

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
    public IActionResult Create(
        [FromBody] RoomCreateInput input,
        [FromHeader(Name = "X-SignalR-ConnectionId")] string signalRConnectionId = null,
        [FromHeader(Name = "Authorization")] string authorizationToken = null)
    {
        try
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(signalRConnectionId))
            {
                _logger.LogInformation($"Create: Status 400. RoomName: {input.RoomName}, RoomPassword: {input.RoomPassword}, Username: {input.Username}");
                return StatusCode(StatusCodes.Status400BadRequest);
            }

            bool isUserConnectedInOtherRoom = _userRepository.GetUserByAuthorizationToken(authorizationToken) != null;

            if (isUserConnectedInOtherRoom)
            {
                _logger.LogInformation($"Create: Status 401. RoomName: {input.RoomName}, RoomPassword: {input.RoomPassword}, Username: {input.Username}");
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            Room room = _roomHandler.CreateRoom(input);

            if (room == null)
            {
                string roomPasswordInfo = input.RoomPassword.Length > 0 ? input.RoomPassword : "<No password>";

                _logger.LogInformation($"Create: Status 409. RoomName: {input.RoomName}, RoomPassword: {roomPasswordInfo}, Username: {input.Username}");
                return StatusCode(StatusCodes.Status409Conflict);
            }

            RoomCreateOutput output = new RoomCreateOutput()
            {
                RoomHash = room.Hash
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

            RoomDTO roomDTO = _mapper.Map<RoomDTO>(room);

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
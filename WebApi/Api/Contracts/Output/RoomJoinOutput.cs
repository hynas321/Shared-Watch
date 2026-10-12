using WebApi.Application.DTO;
using WebApi.Core.Entities;
using WebApi.Core.Entities.In_memory;

namespace WebApi.Api.Contracts.Output;

public class RoomJoinOutput
{
    public bool IsAdmin { get; set; }

    public List<ChatMessage>? ChatMessages { get; set; }
    public List<PlaylistVideo>? PlaylistVideos { get; set; }
    public List<UserDTO>? Users { get; set; }
    public RoomSettingsDTO? RoomSettings { get; set; }
    public UserPermissions? UserPermissions { get; set; }
    public VideoPlayer? VideoPlayer { get; set; }
}

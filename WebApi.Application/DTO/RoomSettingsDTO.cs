using WebApi.Core.Entities;
using WebApi.Core.Enums;
namespace WebApi.Application.DTO;
public class RoomSettingsDTO
{
    public string? RoomName { get; set; }
    public RoomTypes RoomType { get; set; }
    public int MaxUsers { get; set; }
    public static RoomSettingsDTO FromEntity(RoomSettings settings) => new() { RoomName = settings.RoomName, RoomType = settings.RoomType, MaxUsers = settings.MaxUsers };
}

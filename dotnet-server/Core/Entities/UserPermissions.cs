using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DotnetServer.Core.Entities;

public class UserPermissions
{
    [Key, ForeignKey("Room")]
    public string RoomHash { get; set; }

    public bool CanAddChatMessage { get; set; } = true;
    public bool CanAddVideo { get; set; } = true;
    public bool CanRemoveVideo { get; set; } = true;
    public bool CanStartOrPauseVideo { get; set; } = true;
    public bool CanSkipVideo { get; set; } = true;
}
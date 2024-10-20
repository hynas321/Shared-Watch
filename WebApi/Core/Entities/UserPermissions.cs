using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Core.Entities;

public class UserPermissions
{
    [Key, ForeignKey("Room")]
    public string RoomHash { get; set; }

    public bool CanAddChatMessage { get; set; }
    public bool CanAddVideo { get; set; }
    public bool CanRemoveVideo { get; set; }
    public bool CanStartOrPauseVideo { get; set; }
    public bool CanSkipVideo { get; set; }

    public UserPermissions()
    {
        CanAddChatMessage = true;
        CanAddVideo = true;
        CanRemoveVideo = true;
        CanStartOrPauseVideo = false;
        CanSkipVideo = false;
    }
}
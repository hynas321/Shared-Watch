namespace DotnetServer.Core.Entities;

public class UserPermissions {
 
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
        CanStartOrPauseVideo = true;
        CanSkipVideo = false;
    }
}
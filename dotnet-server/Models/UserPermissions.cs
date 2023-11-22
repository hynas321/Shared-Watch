public class UserPermissions {
 
    public bool canAddChatMessage { get; set; }
    public bool canAddVideo { get; set; }
    public bool canRemoveVideo { get; set; }
    public bool canStartOrPauseVideo { get; set; }
    public bool canSkipVideo { get; set; }

    public UserPermissions()
    {
        canAddChatMessage = true;
        canAddVideo = true;
        canRemoveVideo = true;
        canStartOrPauseVideo = true;
        canSkipVideo = false;
    }
}
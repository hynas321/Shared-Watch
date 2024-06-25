public interface IUserPermissions
{
    bool CanAddChatMessage { get; set; }
    bool CanAddVideo { get; set; }
    bool CanRemoveVideo { get; set; }
    bool CanStartOrPauseVideo { get; set; }
    bool CanSkipVideo { get; set; }
}
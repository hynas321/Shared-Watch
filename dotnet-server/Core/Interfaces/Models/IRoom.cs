public interface IRoom
{
    string RoomHash { get; set; }
    IEnumerable<string> AdminTokens { get; set; }
    IEnumerable<IChatMessage> ChatMessages { get; set; }
    IEnumerable<IPlaylistVideo> PlaylistVideos { get; set; }
    IEnumerable<IUser> Users { get; set; }
    IRoomSettings RoomSettings { get; set; }
    IUserPermissions UserPermissions { get; set; }
    IVideoPlayerState VideoPlayerState { get; set; }
}
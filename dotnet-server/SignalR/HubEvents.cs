public static class HubEvents {
    //Connection
    public const string OnReceiveConnectionId = "OnReceiveConnectionId";

    //Room
    public const string KickOut = "KickOut";
    public const string SetAdminStatus = "SetAdminStatus";
    public const string OnJoinRoom = "OnJoinRoom";
    public const string OnLeaveRoom = "OnLeaveRoom";
    public const string OnKickOut = "OnKickOut";
    public const string OnSetAdminStatus = "OnSetAdminStatus";

    //VideoPlayer
    public const string SetIsVideoPlaying = "SetIsVideoPlaying";
    public const string SetPlayedSeconds = "SetPlayedSeconds";
    public const string OnSetVideoUrl = "OnSetVideoUrl";
    public const string OnSetIsVideoPlaying = "OnSetIsVideoPlaying";
    public const string OnSetPlayedSeconds = "OnSetPlayedSeconds";

    //Chat
    public const string AddChatMessage = "AddChatMessage";
    public const string OnAddChatMessage = "OnAddChatMessage";

    //Playlist
    public const string AddPlaylistVideo = "AddPlaylistVideo";
    public const string DeletePlaylistVideo = "DeletePlaylistVideo";
    public const string OnAddPlaylistVideo = "OnAddPlaylistVideo";
    public const string OnDeletePlaylistVideo = "OnDeletePlaylistVideo";

    //RoomSettings
    public const string SetRoomPassword = "SetRoomPassword";
    public const string OnSetRoomPassword = "OnSetRoomPassword";

    //UserPermissions
    public const string SetUserPermissions = "SetUserPermissions";
    public const string OnSetUserPermissions = "OnSetUserPermissions";
}
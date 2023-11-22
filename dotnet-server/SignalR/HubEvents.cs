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

    //Chat
    public const string AddChatMessage = "AddChatMessage";
    public const string OnAddChatMessage = "OnAddChatMessage";

    //Playlist
    public const string AddPlaylistVideo = "AddPlaylistVideo";
    public const string DeletePlaylistVideo = "DeletePlaylistVideo";
    public const string OnAddPlaylistVideo = "OnAddPlaylistVideo";
    public const string OnDeletePlaylistVideo = "OnDeletePlaylistVideo";

    //Settings
}
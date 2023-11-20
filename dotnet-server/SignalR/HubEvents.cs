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
    public const string AddQueuedVideo = "AddQueuedVideo";
    public const string DeleteQueuedVideo = "DeleteQueuedVideo";
    public const string OnAddQueuedVideo = "OnAddQueuedVideo";
    public const string OnDeleteQueuedVideo = "OnDeleteQueuedVideo";

    //Settings
}
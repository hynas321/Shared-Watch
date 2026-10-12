namespace WebApi.SignalR;

public static class HubMessages {
    public const string OnReceiveConnectionId = "OnReceiveConnectionId";
    public const string RefreshConnectionPresence = "RefreshConnectionPresence";

    public const string KickOut = "KickOut";
    public const string SetAdminStatus = "SetAdminStatus";
    public const string OnJoinRoom = "OnJoinRoom";
    public const string OnLeaveRoom = "OnLeaveRoom";
    public const string OnKickOut = "OnKickOut";
    public const string OnSetAdminStatus = "OnSetAdminStatus";

    public const string SetIsVideoPlaying = "SetIsVideoPlaying";
    public const string SetPlayedSeconds = "SetPlayedSeconds";
    public const string OnSetVideoUrl = "OnSetVideoUrl";
    public const string OnSetIsVideoPlaying = "OnSetIsVideoPlaying";
    public const string OnSetPlayedSeconds = "OnSetPlayedSeconds";
    public const string GetPlaybackSnapshot = "GetPlaybackSnapshot";
    public const string NotifyVideoEnded = "NotifyVideoEnded";
    public const string OnPlaybackSync = "OnPlaybackSync";

    public const string AddChatMessage = "AddChatMessage";
    public const string OnAddChatMessage = "OnAddChatMessage";

    public const string AddPlaylistVideo = "AddPlaylistVideo";
    public const string DeletePlaylistVideo = "DeletePlaylistVideo";
    public const string ReorderPlaylistVideos = "ReorderPlaylistVideos";
    public const string OnAddPlaylistVideo = "OnAddPlaylistVideo";
    public const string OnDeletePlaylistVideo = "OnDeletePlaylistVideo";
    public const string OnPlaylistReordered = "OnPlaylistReordered";

    public const string SetRoomPassword = "SetRoomPassword";
    public const string OnSetRoomPassword = "OnSetRoomPassword";

    public const string SetUserPermissions = "SetUserPermissions";
    public const string OnSetUserPermissions = "OnSetUserPermissions";

    public const string OnReceiveJwt = "OnReceiveJwt";
}

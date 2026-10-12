export class HubMessages {
  static RefreshConnectionPresence: string = "RefreshConnectionPresence";
  static KickOut: string = "KickOut";
  static SetAdminStatus: string = "SetAdminStatus";
  static OnJoinRoom: string = "OnJoinRoom";
  static OnLeaveRoom: string = "OnLeaveRoom";
  static OnKickOut: string = "OnKickOut";
  static OnSetAdminStatus: string = "OnSetAdminStatus";

  static SetIsVideoPlaying: string = "SetIsVideoPlaying";
  static SetPlayedSeconds: string = "SetPlayedSeconds";
  static OnSetVideoUrl: string = "OnSetVideoUrl";
  static OnSetIsVideoPlaying: string = "OnSetIsVideoPlaying";
  static OnSetPlayedSeconds: string = "OnSetPlayedSeconds";
  static GetPlaybackSnapshot: string = "GetPlaybackSnapshot";
  static NotifyVideoEnded: string = "NotifyVideoEnded";
  static OnPlaybackSync: string = "OnPlaybackSync";

  static AddChatMessage: string = "AddChatMessage";
  static OnAddChatMessage: string = "OnAddChatMessage";

  static AddPlaylistVideo: string = "AddPlaylistVideo";
  static DeletePlaylistVideo: string = "DeletePlaylistVideo";
  static ReorderPlaylistVideos: string = "ReorderPlaylistVideos";
  static OnAddPlaylistVideo: string = "OnAddPlaylistVideo";
  static OnDeletePlaylistVideo: string = "OnDeletePlaylistVideo";
  static OnPlaylistReordered: string = "OnPlaylistReordered";

  static SetRoomPassword: string = "SetRoomPassword";
  static OnSetRoomPassword: string = "OnSetRoomPassword";

  static SetUserPermissions: string = "SetUserPermissions";
  static OnSetUserPermissions: string = "OnSetUserPermissions";

  static OnReceiveJwt: string = "OnReceiveJwt";
}

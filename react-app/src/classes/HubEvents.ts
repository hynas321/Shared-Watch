export class HubEvents {
  //Connection
 static OnReceiveConnectionId: string = "OnReceiveConnectionId";

  //User
  static KickOut: string = "KickOut";
  static SetAdminStatus: string = "SetAdminStatus";
  static OnJoinRoom: string = "OnJoinRoom";
  static OnLeaveRoom: string = "OnLeaveRoom";
  static OnKickOut: string = "OnKickOut";
  static OnSetAdminStatus: string = "OnSetAdminStatus";

  //VideoPlayer
  static SetIsVideoPlaying = "SetIsVideoPlaying";
  static SetPlayedSeconds = "SetPlayedSeconds";
  static OnSetIsVideoPlaying = "OnSetIsVideoPlaying";
  static OnSetPlayedSeconds = "OnSetPlayedSeconds";

  //Chat
  static AddChatMessage: string = "AddChatMessage";
  static OnAddChatMessage: string = "OnAddChatMessage";

  //Playlist
  static AddPlaylistVideo: string = "AddPlaylistVideo";
  static DeletePlaylistVideo: string = "DeletePlaylistVideo";
  //static StartPlaylistHandler: string = "StartPlaylistHandler";
  static OnAddPlaylistVideo: string = "OnAddPlaylistVideo";
  static OnDeletePlaylistVideo: string = "OnDeletePlaylistVideo";

  //RoomSettings
  static SetRoomPassword: string = "SetRoomPassword";
  static OnSetRoomPassword: string = "OnSetRoomPassword";

  //UserPermissions
  static SetUserPermissions: string = "SetUserPermissions";
  static OnSetUserPermissions: string = "OnSetUserPermissions";

}
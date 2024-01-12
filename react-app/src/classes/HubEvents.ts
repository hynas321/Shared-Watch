export class HubEvents {
  //Connection
  static OnReceiveConnectionId: string = "OnReceiveConnectionId";
  static SendHeartbeat: string = "SendHeartbeat";
  static OnSendHeartbeat: string = "OnSendHeartbeat";

  //Rooms
  static onRoomUpdated: string = "OnRoomUpdated";
  static onListOfRoomsUpdated: string = "OnListOfRoomsUpdated";

  //User
  static KickOut: string = "KickOut";
  static SetAdminStatus: string = "SetAdminStatus";
  static OnJoinRoom: string = "OnJoinRoom";
  static OnLeaveRoom: string = "OnLeaveRoom";
  static OnKickOut: string = "OnKickOut";
  static OnSetAdminStatus: string = "OnSetAdminStatus";

  //VideoPlayer
  static SetIsVideoPlaying: string = "SetIsVideoPlaying";
  static SetPlayedSeconds: string = "SetPlayedSeconds";
  static OnSetVideoUrl: string = "OnSetVideoUrl";
  static OnSetIsVideoPlaying: string = "OnSetIsVideoPlaying";
  static OnSetPlayedSeconds: string = "OnSetPlayedSeconds";

  //Chat
  static AddChatMessage: string = "AddChatMessage";
  static OnAddChatMessage: string = "OnAddChatMessage";

  //Playlist
  static AddPlaylistVideo: string = "AddPlaylistVideo";
  static DeletePlaylistVideo: string = "DeletePlaylistVideo";
  static OnAddPlaylistVideo: string = "OnAddPlaylistVideo";
  static OnDeletePlaylistVideo: string = "OnDeletePlaylistVideo";

  //RoomSettings
  static SetRoomPassword: string = "SetRoomPassword";
  static OnSetRoomPassword: string = "OnSetRoomPassword";

  //UserPermissions
  static SetUserPermissions: string = "SetUserPermissions";
  static OnSetUserPermissions: string = "OnSetUserPermissions";

}
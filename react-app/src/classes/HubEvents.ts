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

  //Chat
  static AddChatMessage: string = "AddChatMessage";
  static OnAddChatMessage: string = "OnAddChatMessage";

  //Playlist
  static AddQueuedVideo: string = "AddQueuedVideo";
  static DeleteQueuedVideo: string = "DeleteQueuedVideo";
  static OnAddQueuedVideo: string = "OnAddQueuedVideo";
  static OnDeleteQueuedVideo: string = "OnDeleteQueuedVideo";
}
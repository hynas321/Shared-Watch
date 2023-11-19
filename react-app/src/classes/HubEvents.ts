export class HubEvents {
  //Room
  static JoinRoom: string = "JoinRoom";
  static LeaveRoom: string = "LeaveRoom";
  static OnJoinRoom: string = "OnJoinRoom";
  static OnLeaveRoom: string = "OnLeaveRoom";

  //Chat
  static AddChatMessage: string = "AddChatMessage";
  static OnAddChatMessage: string = "OnAddChatMessage";

  //Playlist
  static AddQueuedVideo: string = "AddQueuedVideo";
  static DeleteQueuedVideo: string = "DeleteQueuedVideo";
  static OnAddQueuedVideo: string = "OnAddQueuedVideo";
  static OnDeleteQueuedVideo: string = "OnDeleteQueuedVideo";
}
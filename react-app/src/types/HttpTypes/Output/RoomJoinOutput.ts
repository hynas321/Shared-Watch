import { ChatMessage } from "../../ChatMessage";
import { PlaylistVideo } from "../../PlaylistVideo";
import { UserPermissions } from "../../UserPermissions";
import { User } from "../../User";
import { VideoPlayerState } from "../../VideoPlayerState";
import { RoomSettings } from "../../RoomSettings";

export type RoomJoinOutput = {
  //User fields
  authorizationToken: string,
  isAdmin: boolean,
  //Room fields
  chatMessages: ChatMessage[],
  playlistVideos: PlaylistVideo[],
  users: User[],
  roomSettings: RoomSettings,
  userPermissions: UserPermissions,
  videoPlayerState: VideoPlayerState
}
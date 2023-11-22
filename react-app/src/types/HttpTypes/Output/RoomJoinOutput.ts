import { ChatMessage } from "../../ChatMessage";
import { PlaylistVideo } from "../../PlaylistVideo";
import { UserPermissions } from "../../UserPermissions";
import { User } from "../../User";
import { VideoPlayerState } from "../../VideoPlayerSettings";

export type RoomJoinOutput = {
  authorizationToken: string,
  isAdmin: boolean,
  chatMessages: ChatMessage[],
  playlistVideos: PlaylistVideo[],
  users: User[],
  roomSettings: UserPermissions,
  videoPlayerSettings: VideoPlayerState
}
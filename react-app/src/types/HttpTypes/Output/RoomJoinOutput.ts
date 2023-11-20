import { ChatMessage } from "../../ChatMessage";
import { QueuedVideo } from "../../QueuedVideo";
import { RoomSettings } from "../../RoomSettings";
import { User } from "../../User";
import { VideoPlayerSettings } from "../../VideoPlayerSettings";

export type RoomJoinOutput = {
  authorizationToken: string,
  isAdmin: boolean,
  chatMessages: ChatMessage[],
  queuedVideos: QueuedVideo[],
  users: User[],
  roomSettings: RoomSettings,
  videoPlayerSettings: VideoPlayerSettings
}
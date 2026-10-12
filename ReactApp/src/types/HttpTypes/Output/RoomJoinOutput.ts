import { ChatMessage } from "../../ChatMessage";
import { PlaylistVideo } from "../../PlaylistVideo";
import { UserPermissions } from "../../UserPermissions";
import { User } from "../../User";
import { VideoPlayer } from "../../VideoPlayer";
import { RoomSettings } from "../../RoomSettings";

export interface RoomJoinOutput {
  isAdmin: boolean;
  chatMessages: ChatMessage[];
  playlistVideos: PlaylistVideo[];
  users: User[];
  roomSettings: RoomSettings;
  userPermissions: UserPermissions;
  videoPlayer: VideoPlayer;
}

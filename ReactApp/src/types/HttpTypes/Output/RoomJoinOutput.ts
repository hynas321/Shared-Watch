import { ChatMessage } from "../../ChatMessage";
import { PlaylistVideo } from "../../PlaylistVideo";
import { UserPermissions } from "../../UserPermissions";
import { User } from "../../User";
import { VideoPlayer } from "../../VideoPlayer";
import { RoomSettings } from "../../RoomSettings";

export interface RoomJoinOutput {
  //User fields
  authorizationToken: string;
  isAdmin: boolean;
  //Room fields
  chatMessages: ChatMessage[];
  playlistVideos: PlaylistVideo[];
  users: User[];
  roomSettings: RoomSettings;
  userPermissions: UserPermissions;
  videoPlayer: VideoPlayer;
}

import { ChatMessage } from "../types/ChatMessage";
import { User } from "../types/User";
import { UserPermissions } from "../types/UserPermissions";
import { signal } from "@preact/signals-react";
import { VideoPlayerState } from "../types/VideoPlayerSettings";
import { RoomTypesEnum } from "../enums/RoomTypesEnum";
import { PanelsEnum } from "../enums/PanelsEnum";
import { PlaylistVideo } from "../types/PlaylistVideo";

export function createAppState() {
  //signalR
  const connectionId = signal<string | null>("");

  //User state
  const username = signal<string>("");
  const isAdmin = signal<boolean>(false);
  const isInRoom = signal<boolean>(false);

  //Room
  const roomHash = signal<string>("");
  const roomName = signal<string>("");
  const roomType = signal<RoomTypesEnum>(RoomTypesEnum.public);
  const password = signal<string>("");

  //Control panel data
  const chatMessages = signal<ChatMessage[]>([]);
  const playlistVideos = signal<PlaylistVideo[]>([]);
  const users = signal<User[]>([]);
  const roomSettings = signal<UserPermissions | null>(null);
  const videoPlayerSettings = signal<VideoPlayerState | null>(null);

  //Auxilary control panel data
  const unreadChatMessagesCount = signal<number>(0);
  const activePanel = signal<PanelsEnum>(PanelsEnum.Chat);

  return {
    connectionId,
    username, isAdmin, isInRoom,
    roomHash, roomName, roomType, password,
    chatMessages, playlistVideos: playlistVideos, users, roomSettings, videoPlayerSettings,
    unreadChatMessagesCount, activePanel
  }
}


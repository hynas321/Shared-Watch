import { ChatMessage } from "../types/ChatMessage";
import { User } from "../types/User";
import { UserPermissions } from "../types/UserPermissions";
import { signal } from "@preact/signals-react";
import { VideoPlayer } from "../types/VideoPlayer";
import { PanelsEnum } from "../enums/PanelsEnum";
import { PlaylistVideo } from "../types/PlaylistVideo";
import { RoomTypesEnum } from "../enums/RoomTypesEnum";

export function createAppState() {
  //signalR
  const connectionId = signal<string | null | undefined>(undefined);

  //User state
  const username = signal<string>("");
  const isAdmin = signal<boolean>(false);
  const isInRoom = signal<boolean>(false);

  //Room
  const roomHash = signal<string>("");

  //Room settings
  const roomName = signal<string>("");
  const roomPassword = signal<string>("");
  const roomType = signal<RoomTypesEnum>(RoomTypesEnum.public);
  const maxUsers = signal<number>(10);

  //Control panel data
  const chatMessages = signal<ChatMessage[]>([]);
  const playlistVideos = signal<PlaylistVideo[]>([]);
  const users = signal<User[]>([]);
  const userPermissions = signal<UserPermissions | null>(null);
  const videoPlayer = signal<VideoPlayer | null>(null);

  //Auxilary control panel data
  const unreadChatMessagesCount = signal<number>(0);
  const activePanel = signal<PanelsEnum>(PanelsEnum.Chat);
  const maxPlaylistVideos = signal<number>(10);

  //Url
  const joinedViaView = signal<boolean>(false);

  return {
    connectionId,
    username, isAdmin, isInRoom,
    roomHash, roomName, roomPassword, roomType, maxUsers,
    chatMessages, playlistVideos, users, userPermissions, videoPlayer: videoPlayer,
    unreadChatMessagesCount, activePanel, maxPlaylistVideos,
    joinedViaView
  }
}


import { ChatMessage } from "../types/ChatMessage";
import { QueuedVideo } from "../types/QueuedVideo";
import { User } from "../types/User";
import { RoomSettings } from "../types/RoomSettings";
import { signal } from "@preact/signals-react";
import { VideoPlayerSettings } from "../types/VideoPlayerSettings";
import { RoomTypesEnum } from "../enums/RoomTypesEnum";
import { PanelsEnum } from "../enums/PanelsEnum";

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
  const queuedVideos = signal<QueuedVideo[]>([]);
  const users = signal<User[]>([]);
  const roomSettings = signal<RoomSettings | null>(null);
  const videoPlayerSettings = signal<VideoPlayerSettings | null>(null);

  //Auxilary control panel data
  const unreadChatMessagesCount = signal<number>(0);
  const activePanel = signal<PanelsEnum>(PanelsEnum.Chat);

  return {
    connectionId,
    username, isAdmin, isInRoom,
    roomHash, roomName, roomType, password,
    chatMessages, queuedVideos, users, roomSettings, videoPlayerSettings,
    unreadChatMessagesCount, activePanel
  }
}


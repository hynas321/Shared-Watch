import { ChatMessage } from "../types/ChatMessage";
import { QueuedVideo } from "../types/QueuedVideo";
import { User } from "../types/User";
import { RoomSettings } from "../types/RoomSettings";
import { signal } from "@preact/signals-react";
import { VideoPlayerSettings } from "../types/VideoPlayerSettings";
import { RoomTypesEnum } from "../enums/RoomTypesEnum";

export function createAppState() {
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
  const playlistVideos = signal<QueuedVideo[]>([]);
  const users = signal<User[]>([]);
  const roomSettings = signal<RoomSettings | null>(null);
  const videoPlayerSettings = signal<VideoPlayerSettings | null>(null);

  return {
    username, isAdmin, isInRoom,
    roomHash, roomName, roomType, password,
    chatMessages, playlistVideos, users, roomSettings, videoPlayerSettings
  }
}


import { PlaylistVideo } from "./PlaylistVideo";

export interface VideoPlayer {
  playlistVideo: PlaylistVideo | null;
  isPlaying: boolean;
  currentTime: number;
  serverTimestampUnixMs?: number;
  revision?: number;
}

import { PlaylistVideo } from "./PlaylistVideo";

export interface VideoPlayer {
  playlistVideo: PlaylistVideo;
  isPlaying: boolean;
  currentTime: number;
}

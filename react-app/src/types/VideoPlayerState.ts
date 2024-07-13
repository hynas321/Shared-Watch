import { PlaylistVideo } from "./PlaylistVideo"

export interface VideoPlayerState {
  playlistVideo: PlaylistVideo,
  isPlaying: boolean,
  currentTime: number
}
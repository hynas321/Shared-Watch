import { PlaylistVideo } from "./PlaylistVideo"

export type VideoPlayerState = {
  playlistVideo: PlaylistVideo,
  isPlaying: boolean,
  currentTime: number,
  duration: number
}
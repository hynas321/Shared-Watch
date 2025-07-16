import { PlaylistVideo } from "../../types/PlaylistVideo";
import VideoOnPlaylist from "./PlaylistVideo";

interface Props {
  video: PlaylistVideo;
  index: number;
}

export default function PlaylistVideoItem({ video, index }: Props) {
  const url = video.url.startsWith("http") ? video.url : `http://${video.url}`;
  const highlightStyle = index === 0 ? { backgroundColor: "#DAF7A6" } : {};

  return (
    <a
      className="border border-secondary list-group-item bg-muted border-2 a-video"
      href={url}
      target="_blank"
      rel="noopener noreferrer"
      style={highlightStyle}
    >
      <VideoOnPlaylist index={index} playlistVideo={video} />
    </a>
  );
}

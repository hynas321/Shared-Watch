import { PlaylistVideo } from "../../types/PlaylistVideo";
import VideoOnPlaylist from "./PlaylistVideo";
import { getPlayableVideoUrl } from "../../classes/helpers/VideoUrl";
import { appState } from "../../context/AppContext";

interface Props {
  video: PlaylistVideo;
  index: number;
  draggable?: boolean;
  isDragOver?: boolean;
  onDragStart?: () => void;
  onDragOver?: () => void;
  onDrop?: () => void;
  onDragEnd?: () => void;
}

export default function PlaylistVideoItem({
  video,
  index,
  draggable = false,
  isDragOver = false,
  onDragStart,
  onDragOver,
  onDrop,
  onDragEnd,
}: Props) {
  const isCurrentVideo = appState.videoPlayer.value?.playlistVideo?.hash === video.hash;
  const itemStyle = {
    backgroundColor: isCurrentVideo ? "#DAF7A6" : "#FFFFFF",
    color: "#212529",
  };
  const isUploadedVideo = !video.url.startsWith("http");
  const content = <VideoOnPlaylist index={index} playlistVideo={video} />;

  if (isUploadedVideo) {
    return (
      <div
        className={`border border-secondary list-group-item bg-muted border-2${
          isCurrentVideo ? " playlist-video-current" : ""
        }${isDragOver ? " playlist-video-drag-over" : ""}`}
        aria-current={isCurrentVideo ? "true" : undefined}
        draggable={draggable}
        onDragStart={onDragStart}
        onDragOver={(event) => { event.preventDefault(); onDragOver?.(); }}
        onDrop={(event) => { event.preventDefault(); onDrop?.(); }}
        onDragEnd={onDragEnd}
        style={itemStyle}
      >
        {content}
      </div>
    );
  }

  return (
    <a
      className={`border border-secondary list-group-item bg-muted border-2 a-video${
        isCurrentVideo ? " playlist-video-current" : ""
      }${isDragOver ? " playlist-video-drag-over" : ""}`}
      href={getPlayableVideoUrl(video.url)}
      target="_blank"
      rel="noopener noreferrer"
      style={itemStyle}
      aria-current={isCurrentVideo ? "true" : undefined}
      draggable={draggable}
      onDragStart={onDragStart}
      onDragOver={(event) => { event.preventDefault(); onDragOver?.(); }}
      onDrop={(event) => { event.preventDefault(); onDrop?.(); }}
      onDragEnd={onDragEnd}
    >
      {content}
    </a>
  );
}

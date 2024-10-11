import { BsFillXCircleFill } from "react-icons/bs";
import { HubMessages } from "../classes/constants/HubMessages";
import { appHub, appState } from "../context/AppContext";
import { PlaylistVideo } from "../types/PlaylistVideo";
import VideoIcon from "./../assets/video-icon.png";
import Button from "./shared/Button";

export interface VideoOnPlaylistProps {
  index: number;
  playlistVideo: PlaylistVideo;
}

const videoThumbnailStyle = {
  width: "40px",
  height: "40px",
};

export default function VideoOnPlaylist({ index, playlistVideo }: VideoOnPlaylistProps) {
  const handleDeleteClick = (event: React.MouseEvent, index: number) => {
    event.preventDefault();
    const videoHash = appState.playlistVideos.value[index].hash;
    appHub.invoke(HubMessages.DeletePlaylistVideo, appState.roomHash.value, videoHash);
  };

  const canRemoveVideo = appState.userPermissions.value?.canRemoveVideo || appState.isAdmin.value;

  return (
    <div className="row">
      <div className="col-auto">
        <img
          src={playlistVideo.thumbnailUrl || VideoIcon}
          alt="Video Thumbnail"
          style={videoThumbnailStyle}
        />
      </div>
      <div className="d-flex col justify-content-between align-items-center text-secondary">
        <small style={{ wordWrap: "break-word", maxWidth: "200px" }}>
          <b>{playlistVideo.title || playlistVideo.url}</b>
        </small>
        {canRemoveVideo && (
          <div>
            <Button
              text={<BsFillXCircleFill />}
              classNames="btn btn-danger btn-sm"
              onClick={(event) => handleDeleteClick(event, index)}
            />
          </div>
        )}
      </div>
    </div>
  );
}

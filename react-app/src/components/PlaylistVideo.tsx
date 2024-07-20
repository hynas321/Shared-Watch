import { BsFillXCircleFill } from 'react-icons/bs';
import { HubEvents } from '../classes/HubEvents';
import { LocalStorageManager } from '../classes/LocalStorageManager';
import { appHub, appState } from '../context/AppContext';
import { PlaylistVideo } from '../types/PlaylistVideo'
import VideoIcon from './../assets/video-icon.png'
import Button from './Button';

export interface VideoOnPlaylistProps {
  index: number,
  playlistVideo: PlaylistVideo
}

export default function VideoOnPlaylist({index, playlistVideo}: VideoOnPlaylistProps) {
  const localStorageManager = new LocalStorageManager();

  const videoThumbnailStyle = {
    width: "40px",
    height: "40px"
  };

  const handleDeletePlaylistVideoButtonClick = (event: any, index: number) => {
    event.preventDefault();
    appHub.invoke(
      HubEvents.DeletePlaylistVideo,
      appState.roomHash.value,
      localStorageManager.getAuthorizationToken(),
      appState.playlistVideos.value[index].hash
    );
  }

  return (
    <div className="row">
      <div className="col-auto">
        <img src={playlistVideo.thumbnailUrl === null ? VideoIcon : playlistVideo.thumbnailUrl} alt="Video" style={videoThumbnailStyle} />
      </div>
      <div className="d-flex col justify-content-between align-items-center text-secondary align-items-center">
        <small style={{ wordWrap: 'break-word', maxWidth: '200px' }}>
          <b>{playlistVideo.title === null ? playlistVideo.url : playlistVideo.title}</b>
        </small>
        {(appState.userPermissions.value?.canRemoveVideo || appState.isAdmin.value) && (
          <div>
            <Button
              text={<BsFillXCircleFill />}
              classNames="btn btn-danger btn-sm"
              onClick={() => handleDeletePlaylistVideoButtonClick(event, index)}
            />
          </div>
        )}
      </div>
    </div>
  )
}
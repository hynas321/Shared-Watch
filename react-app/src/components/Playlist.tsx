import { BsPlusCircleFill, BsXCircle } from "react-icons/bs";
import { PlaylistVideo } from "../types/PlaylistVideo";
import Button from "./Button";
import { InputForm } from "./InputForm";
import { useContext, useEffect, useRef, useState } from "react";
import VideoIcon from './../assets/video-icon.png'
import { AppStateContext, RoomHubContext } from "../context/RoomHubContext";
import { HubEvents } from "../classes/HubEvents";
import { LocalStorageManager } from "../classes/LocalStorageManager";

export default function Playlist() {
  const roomHub = useContext(RoomHubContext);
  const appState = useContext(AppStateContext);
  const playlistVideosRef = useRef<HTMLDivElement>(null);

  const [currentVideoUrlText, setCurrentVideoUrlText] = useState<string>("");

  const localStorageManager = new LocalStorageManager();
  
  const videoThumbnailStyle = {
    width: "40px",
    height: "40px"
  };
  
  useEffect(() => {
    if (playlistVideosRef.current) {
      playlistVideosRef.current.scrollTop = playlistVideosRef.current.scrollHeight;
    }
  }, [appState.playlistVideos.value]);

  const handleTextInputChange = (text: string) => {
    setCurrentVideoUrlText(text);
  }

  const handleAddVideoUrlButtonClick = () => {
    if (!currentVideoUrlText || currentVideoUrlText?.length === 0) {
      return;
    }

    const newPlaylistVideo: PlaylistVideo = {
      url: currentVideoUrlText,
    };

    roomHub.invoke(HubEvents.AddPlaylistVideo, appState.roomHash.value, localStorageManager.getAuthorizationToken(), newPlaylistVideo);
    setCurrentVideoUrlText("");
  }

  const handleEnterPress = (key: string) => {
    if (key === "Enter") {
      handleAddVideoUrlButtonClick();
    }
  }

  const handleDeletePlaylistVideoButtonClick = (event: any, index: number) => {
    event.preventDefault();    
    roomHub.invoke(HubEvents.DeletePlaylistVideo, appState.roomHash.value, localStorageManager.getAuthorizationToken(), index);
  }

  return (
    <>
      <div className="d-flex mb-3">
        <InputForm
          classNames="form-control rounded-0"
          value={currentVideoUrlText}
          trim={true}
          placeholder="Paste video URL"
          isEnabled={true}
          onChange={handleTextInputChange}
          onKeyDown={handleEnterPress}
        />
        <Button
          text={<BsPlusCircleFill />}
          classNames="btn btn-primary rounded-0"
          onClick={handleAddVideoUrlButtonClick}
        />
      </div>
      <div className="list-group rounded-3 control-panel-list" ref={playlistVideosRef}>
      {
        appState.playlistVideos.value.length !== 0 ? (
          appState.playlistVideos.value.map((playlistVideo, index) => (
            <a 
              key={index}
              className="border border-secondary list-group-item bg-muted border-2 a-video"
              href={playlistVideo.url.startsWith('http') ? playlistVideo.url : `http://${playlistVideo.url}`}
              target={"_blank"}
            >
              <div className="row">
                <div className="col-auto">
                  <img src={VideoIcon} alt="Video Icon" style={videoThumbnailStyle} />
                </div>
                <div className="d-flex col justify-content-between align-items-center text-secondary align-items-center">
                 <small style={{wordWrap: 'break-word', maxWidth: '200px'}}>{playlistVideo.url}</small>
                  <div>
                    <Button
                      text={<BsXCircle/>}
                      classNames="btn btn-outline-danger btn-sm"
                      onClick={() => handleDeletePlaylistVideoButtonClick(event, index)}
                    />
                  </div>
                </div>
              </div>
            </a>
          ))
        ) :
        <h6 className="text-white text-center">No videos to display</h6>
      }
      </div>
    </>
  )
}

import { BsPlusCircleFill, BsXCircle } from "react-icons/bs";
import { QueuedVideo } from "../types/QueuedVideo";
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
  const queuedVideosRef = useRef<HTMLDivElement>(null);

  const [currentVideoUrlText, setCurrentVideoUrlText] = useState<string>("");

  const localStorageManager = new LocalStorageManager();
  
  const videoThumbnailStyle = {
    width: "40px",
    height: "40px"
  };
  
  useEffect(() => {
    if (queuedVideosRef.current) {
      queuedVideosRef.current.scrollTop = queuedVideosRef.current.scrollHeight;
    }
  }, [appState.queuedVideos.value]);

  const handleTextInputChange = (text: string) => {
    setCurrentVideoUrlText(text);
  }

  const handleAddVideoUrlButtonClick = () => {
    if (!currentVideoUrlText || currentVideoUrlText?.length === 0) {
      return;
    }

    const newQueuedVideo: QueuedVideo = {
      url: currentVideoUrlText,
    };

    roomHub.invoke(HubEvents.AddQueuedVideo, appState.roomHash.value, localStorageManager.getAuthorizationToken(), newQueuedVideo);
    setCurrentVideoUrlText("");
  }

  const handleEnterPress = (key: string) => {
    if (key === "Enter") {
      handleAddVideoUrlButtonClick();
    }
  }

  const handleRemoveQueuedVideoButtonClick = (event: any, index: number) => {
    event.preventDefault();    
    roomHub.invoke(HubEvents.DeleteQueuedVideo, appState.roomHash.value, appState.queuedVideos.value[index]);
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
      <div className="list-group rounded-3 control-panel-list" ref={queuedVideosRef}>
      {
        appState.queuedVideos.value.length !== 0 ? (
          appState.queuedVideos.value.map((queuedVideo, index) => (
            <a 
              key={index}
              className="border border-secondary list-group-item bg-muted border-2 a-video"
              href={queuedVideo.url.startsWith('http') ? queuedVideo.url : `http://${queuedVideo.url}`}
              target={"_blank"}
            >
              <div className="row">
                <div className="col-auto">
                  <img src={VideoIcon} alt="Video Icon" style={videoThumbnailStyle} />
                </div>
                <div className="d-flex col justify-content-between align-items-center text-secondary align-items-center">
                 <small style={{wordWrap: 'break-word', maxWidth: '200px'}}>{queuedVideo.url}</small>
                  <div>
                    <Button
                      text={<BsXCircle/>}
                      classNames="btn btn-outline-danger btn-sm"
                      onClick={() => handleRemoveQueuedVideoButtonClick(event, index)}
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

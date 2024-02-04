import { BsFillXCircleFill, BsPlusCircleFill } from "react-icons/bs";
import { PlaylistVideo } from "../types/PlaylistVideo";
import Button from "./Button";
import { InputField } from "./InputField";
import { useContext, useEffect, useRef, useState } from "react";
import VideoIcon from './../assets/video-icon.png'
import { AppStateContext, AppHubContext } from "../context/AppContext";
import { HubEvents } from "../classes/HubEvents";
import { LocalStorageManager } from "../classes/LocalStorageManager";
import { RoomHelper } from "../classes/RoomHelper";

export default function Playlist() {
  const appHub = useContext(AppHubContext);
  const appState = useContext(AppStateContext);
  const playlistVideosRef = useRef<HTMLDivElement>(null);

  const [currentVideoUrlText, setCurrentVideoUrlText] = useState<string>("");
  const [isInputFormEnabled, setIsInputFormEnabled] = useState<boolean>(true);
  const [inputFormPlaceholderText, setInputFormPlaceholderText] = useState<string>("Paste Youtube Video URL");

  const localStorageManager = new LocalStorageManager();
  const roomHelper = new RoomHelper();
  
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

  const handleAddVideoUrlButtonClick = async () => {
    if (!currentVideoUrlText || currentVideoUrlText?.length === 0) {
      return;
    }

    const isUrlValid = roomHelper.checkIfIsYouTubeVideoLink(currentVideoUrlText);

    if (!isUrlValid) {
      setIsInputFormEnabled(false);
      setInputFormPlaceholderText("Incorrect Youtube Video URL");
  
      setTimeout(() => {
        setIsInputFormEnabled(true);
        setInputFormPlaceholderText("Paste Youtube Video URL");
      }, 1500);

      setCurrentVideoUrlText("");
      return;
    }

    const newPlaylistVideo: PlaylistVideo = {
      url: currentVideoUrlText,
      duration: 5
    };

    appHub.invoke(HubEvents.AddPlaylistVideo, appState.roomHash.value, localStorageManager.getAuthorizationToken(), newPlaylistVideo);
    setCurrentVideoUrlText("");
  }

  const handleEnterPress = (key: string) => {
    if (key === "Enter") {
      handleAddVideoUrlButtonClick();
    }
  }

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
    <>
      
      {
        (appState.userPermissions.value?.canAddVideo || appState.isAdmin.value) &&
        <div className="d-flex mb-3">
          <InputField
            classNames={`form-control rounded-0 ${!isInputFormEnabled && "border-5 border-danger"}`}
            value={currentVideoUrlText}
            trim={true}
            placeholder={inputFormPlaceholderText}
            isEnabled={isInputFormEnabled}
            maxCharacters={200}
            onChange={handleTextInputChange}
            onKeyDown={handleEnterPress}
          />
          <Button
            text={<BsPlusCircleFill />}
            classNames="btn btn-primary rounded-0"
            onClick={handleAddVideoUrlButtonClick}
          />
        </div>
      }
      <div className="list-group rounded-3 control-panel-list" ref={playlistVideosRef}>
      {
        appState.playlistVideos.value.length !== 0 ? (
          appState.playlistVideos.value.map((playlistVideo, index) => (
            <a 
              key={index}
              className="border border-secondary list-group-item bg-muted border-2 a-video"
              href={playlistVideo.url.startsWith('http') ? playlistVideo.url : `http://${playlistVideo.url}`}
              target={"_blank"}
              style={index === 0 ? { backgroundColor: "#DAF7A6" } : {}}
            >
              <div className="row">
                <div className="col-auto">
                  <img src={playlistVideo.thumbnailUrl === null ? VideoIcon : playlistVideo.thumbnailUrl} alt="Video" style={videoThumbnailStyle} />
                </div>
                <div className="d-flex col justify-content-between align-items-center text-secondary align-items-center">
                 <small style={{wordWrap: 'break-word', maxWidth: '200px'}}>
                  <b>{playlistVideo.title === null ? playlistVideo.url : playlistVideo.title}</b>
                </small>
                 {
                    (appState.userPermissions.value?.canRemoveVideo || appState.isAdmin.value) &&
                    <div>
                      <Button
                        text={<BsFillXCircleFill/>}
                        classNames="btn btn-danger btn-sm"
                        onClick={() => handleDeletePlaylistVideoButtonClick(event, index)}
                      />
                    </div>
                  }
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

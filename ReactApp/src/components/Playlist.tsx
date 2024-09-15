import { BsPlusCircleFill } from "react-icons/bs";
import { PlaylistVideo } from "../types/PlaylistVideo";
import { useContext, useEffect, useRef, useState } from "react";
import { AppStateContext, AppHubContext } from "../context/AppContext";
import { HubMessages } from "../classes/constants/HubMessages";
import { RoomHelper } from "../classes/helpers/RoomHelper";
import VideoOnPlaylist from "./PlaylistVideo";
import Button from "./shared/Button";
import { InputField } from "./shared/InputField";

export default function Playlist() {
  const appHub = useContext(AppHubContext);
  const appState = useContext(AppStateContext);
  const playlistVideosRef = useRef<HTMLDivElement>(null);

  const [currentVideoUrlText, setCurrentVideoUrlText] = useState<string>("");
  const [isInputFormEnabled, setIsInputFormEnabled] = useState<boolean>(true);
  const [inputFormPlaceholderText, setInputFormPlaceholderText] = useState<string>("Paste Youtube Video URL");

  const roomHelper = RoomHelper.getInstance();
  
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

    appHub.invoke(HubMessages.AddPlaylistVideo, appState.roomHash.value, newPlaylistVideo);
    setCurrentVideoUrlText("");
  }

  const handleEnterPress = (key: string) => {
    if (key === "Enter") {
      handleAddVideoUrlButtonClick();
    }
  }

  return (
    <>
      {(appState.userPermissions.value?.canAddVideo || appState.isAdmin.value) && (
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
      )}
      <div className="list-group rounded-3 control-panel-list" ref={playlistVideosRef}>
        {appState.playlistVideos.value.length !== 0 ? (
          appState.playlistVideos.value.map((playlistVideo: PlaylistVideo, index: number) => (
            <a 
              key={index}
              className="border border-secondary list-group-item bg-muted border-2 a-video"
              href={playlistVideo.url.startsWith('http') ? playlistVideo.url : `http://${playlistVideo.url}`}
              target="_blank"
              style={index === 0 ? { backgroundColor: "#DAF7A6" } : {}}
            >
              <VideoOnPlaylist
                index={index}
                playlistVideo={playlistVideo}
              />
            </a>
          ))
        ) : (
          <h6 className="text-white text-center">No videos to display</h6>
        )}
      </div>
    </>
  );
}

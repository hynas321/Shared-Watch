import { BsPlusCircleFill } from "react-icons/bs";
import { useContext, useEffect, useRef, useState } from "react";
import { AppHubContext, AppStateContext } from "../../context/AppContext";
import { HubMessages } from "../../classes/constants/HubMessages";
import { InputField } from "../shared/InputField";
import Button from "../shared/Button";
import PlaylistVideoItem from "./PlaylistVideoItem";
import { PlaylistVideo } from "../../types/PlaylistVideo";
import { useRoomHelper } from "../../hooks/useRoomHelper";

export default function Playlist() {
  const appState = useContext(AppStateContext);
  const appHub = useContext(AppHubContext);
  const playlistVideosRef = useRef<HTMLDivElement>(null);
  const { checkIfIsYouTubeVideoLink } = useRoomHelper();

  const [videoUrl, setVideoUrl] = useState("");
  const [isEnabled, setIsEnabled] = useState(true);
  const [placeholder, setPlaceholder] = useState("Paste Youtube Video URL");

  useEffect(() => {
    if (playlistVideosRef.current) {
      playlistVideosRef.current.scrollTop = playlistVideosRef.current.scrollHeight;
    }
  }, [appState.playlistVideos.value]);

  const handleAddVideo = async () => {
    if (!videoUrl.trim()) return;

    if (!checkIfIsYouTubeVideoLink(videoUrl)) {
      setIsEnabled(false);
      setPlaceholder("Incorrect Youtube Video URL");
      setTimeout(() => {
        setIsEnabled(true);
        setPlaceholder("Paste Youtube Video URL");
      }, 1500);
      setVideoUrl("");
      return;
    }

    const newVideo: PlaylistVideo = { url: videoUrl, duration: 5 };
    await appHub.invoke(HubMessages.AddPlaylistVideo, appState.roomHash.value, newVideo);
    setVideoUrl("");
  };

  const handleKeyPress = (key: string) => {
    if (key === "Enter") handleAddVideo();
  };

  return (
    <>
      {(appState.userPermissions.value?.canAddVideo || appState.isAdmin.value) && (
        <div className="d-flex mb-3">
          <InputField
            classNames={`form-control rounded-0 ${!isEnabled && "border-5 border-danger"}`}
            value={videoUrl}
            trim
            placeholder={placeholder}
            isEnabled={isEnabled}
            maxCharacters={200}
            onChange={setVideoUrl}
            onKeyDown={handleKeyPress}
          />
          <Button
            text={<BsPlusCircleFill />}
            classNames="btn btn-primary rounded-0"
            onClick={handleAddVideo}
          />
        </div>
      )}

      <div className="list-group rounded-3 control-panel-list" ref={playlistVideosRef}>
        {appState.playlistVideos.value.length ? (
          appState.playlistVideos.value.map((video, i) => (
            <PlaylistVideoItem key={i} index={i} video={video} />
          ))
        ) : (
          <h6 className="text-white text-center">No videos to display</h6>
        )}
      </div>
    </>
  );
}

import ReactPlayer from "react-player";
import { useContext, useEffect, useState } from "react";
import { AppStateContext, RoomHubContext } from "../context/RoomHubContext";
import * as signalR from "@microsoft/signalr";
import { HubEvents } from "../classes/HubEvents";
import { LocalStorageManager } from "../classes/LocalStorageManager";
import { useSignal } from "@preact/signals-react";

export default function VideoPlayer() {
  const appState = useContext(AppStateContext);
  const roomHub = useContext(RoomHubContext);

  const isVideoPlaying = useSignal<boolean>(appState.videoPlayerState.value?.isPlaying ?? false);
  const [userStartedVideo, setUserStartedVideo] = useState(false);
  const [isMobileView, setIsMobileView] = useState(false);

  const localStorageManager = new LocalStorageManager();

  useEffect(() => {
    if (roomHub.getState() !== signalR.HubConnectionState.Connected) {
      return;
    }

    const handleSetIsVideoPlaying = (isPlaying: boolean) => {
      isVideoPlaying.value = isPlaying;

      // Reset userStartedVideo to false when the video is stopped or paused
      if (!isPlaying) {
        setUserStartedVideo(false);
      }
    };

    roomHub.on(HubEvents.OnSetIsVideoPlaying, handleSetIsVideoPlaying);

    return () => {
      roomHub.off(HubEvents.OnSetIsVideoPlaying);
    }
  }, [roomHub.getState(), isVideoPlaying.value]);

  const setUserVideoState = async (newIsPlaying: boolean) => {
    setUserStartedVideo(true);
    await roomHub.invoke(HubEvents.SetIsVideoPlaying, appState.roomHash.value, localStorageManager.getAuthorizationToken(), newIsPlaying);
  };

  const handleStartPauseVideo = () => {
    if (!userStartedVideo) {
      setUserVideoState(true);
    } else {
      setUserVideoState(!isVideoPlaying.value);
    }
  };

  const handleWindowResize = () => {
    setIsMobileView(window.innerWidth <= 576);
  };

  useEffect(() => {
    // Initial check on component mount
    handleWindowResize();

    // Add event listener for window resize
    window.addEventListener("resize", handleWindowResize);

    // Cleanup: remove event listener on component unmount
    return () => {
      window.removeEventListener("resize", handleWindowResize);
    };
  }, []);

  return (
    <>
      <div className="rounded-top-5 bg-dark bg-opacity-50 pt-2 text-center">
        <span className="text-white"><b>Video player</b></span>
      </div>
      <div className={`d-flex justify-content-center rounded-bottom-5 bg-dark bg-opacity-50 pt-2 pb-5 ${isMobileView ? "mobile-view" : ""}`}>
        <ReactPlayer
          url={appState.videoPlayerState.value?.url}
          playing={isVideoPlaying.value}
          controls={true}
          width={isMobileView ? "428px" : "854px"}
          height={isMobileView ? "auto" : "480px"}
          style={{}}
          onStart={handleStartPauseVideo}
          onPause={handleStartPauseVideo}
        />
      </div>
    </>
  );
}
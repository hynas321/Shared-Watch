import ReactPlayer from "react-player";
import { useContext, useEffect, useRef, useState } from "react";
import { AppStateContext, RoomHubContext } from "../context/RoomHubContext";
import * as signalR from "@microsoft/signalr";
import { HubEvents } from "../classes/HubEvents";
import { LocalStorageManager } from "../classes/LocalStorageManager";
import { OnProgressProps } from "react-player/base";

export default function VideoPlayer() {
  const appState = useContext(AppStateContext);
  const roomHub = useContext(RoomHubContext);

  const videoPlayerRef = useRef<ReactPlayer>(null);
  const [videoUrl, setVideoUrl] = useState<string | undefined>(appState.videoPlayerState.value?.playlistVideo.url ?? undefined);
  const [isMobileView, setIsMobileView] = useState<boolean>(false);
  const [isVideoPlaying, setIsVideoPlaying] = useState<boolean>(appState.videoPlayerState.value?.isPlaying ?? false);

  const localStorageManager = new LocalStorageManager();

  useEffect(() => {
    if (roomHub.getState() !== signalR.HubConnectionState.Connected) {
      return;
    }

    roomHub.on(HubEvents.OnSetIsVideoPlaying, (isPlaying: boolean) => {
      if (appState.videoPlayerState.value === null) {
        return;
      }

      appState.videoPlayerState.value.isPlaying = isPlaying;
      setIsVideoPlaying(isPlaying);
    });

    roomHub.on(HubEvents.OnSetPlayedSeconds, (newTime: number) => {
      if (appState.videoPlayerState.value === null) {
        return;
      }

      appState.videoPlayerState.value.currentTime = newTime;
       
      if (videoPlayerRef.current?.getCurrentTime() != null &&
        (videoPlayerRef.current?.getCurrentTime() - newTime > 2 ||
         newTime - videoPlayerRef.current?.getCurrentTime() > 2)) {

        videoPlayerRef.current?.seekTo(newTime, "seconds");
      }
    });

    roomHub.on(HubEvents.OnSetVideoUrl, async (url: string) => {
      if (appState.videoPlayerState.value == null) {
        return;
      }

      appState.videoPlayerState.value.playlistVideo.url = url;
      setVideoUrl(url);
    });

    return () => {
      roomHub.off(HubEvents.OnSetIsVideoPlaying);
      roomHub.off(HubEvents.OnSetPlayedSeconds);
      roomHub.off(HubEvents.OnSetVideoUrl);
    };
  }, [roomHub.getState()]);

  const setUserVideoState = async (isPlaying: boolean) => {
    await roomHub.invoke(
      HubEvents.SetIsVideoPlaying,
      appState.roomHash.value,
      localStorageManager.getAuthorizationToken(),
      isPlaying
    );
  };

  const handleStartVideo = () => {
    setUserVideoState(true);
  };

  const handlePauseVideo = () => {
    setUserVideoState(false);
  };
  

  const handleWindowResize = () => {
    setIsMobileView(window.innerWidth <= 576);
  };

  useEffect(() => {
    handleWindowResize();

    window.addEventListener("resize", handleWindowResize);

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
          ref={videoPlayerRef}
          url={videoUrl}
          playing={isVideoPlaying}
          controls={true}
          width={isMobileView ? "428px" : "854px"}
          height={isMobileView ? "auto" : "480px"}
          style={{}}
          onPlay={() => { handleStartVideo(); console.log("start"); }}
          onPause={() => { handlePauseVideo(); console.log("pause"); }}
          onSeek={(seconds: number) => { console.log(videoPlayerRef.current?.seekTo(seconds, "seconds"))}}
        />
      </div>
    </>
  );
}
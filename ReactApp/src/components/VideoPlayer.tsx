import ReactPlayer from "react-player";
import { useContext, useEffect, useRef, useState } from "react";
import { AppStateContext, AppHubContext } from "../context/AppContext";
import * as signalR from "@microsoft/signalr";
import { HubMessages } from "../classes/constants/HubMessages";
import { OnProgressProps } from "react-player/base";
import { BsCameraVideoOffFill } from "react-icons/bs";

export default function VideoPlayer() {
  const appState = useContext(AppStateContext);
  const appHub = useContext(AppHubContext);

  const videoPlayerRef = useRef<ReactPlayer>(null);
  const [videoUrl, setVideoUrl] = useState<string | undefined>(
    appState.videoPlayer.value?.playlistVideo.url ?? undefined
  );
  const [isMobileView, setIsMobileView] = useState<boolean>(false);
  const [isVideoPlaying, setIsVideoPlaying] = useState<boolean>(
    appState.videoPlayer.value?.isPlaying ?? false
  );
  const [isVideoCurrentTimeDifferenceLarge, setIsVideoCurrentTimeDifferenceLarge] =
    useState<boolean>(false);

  useEffect(() => {
    if (appHub.getState() !== signalR.HubConnectionState.Connected) {
      return;
    }

    const handleSetIsVideoPlaying = (isPlaying: boolean) => {
      if (appState.videoPlayer.value === null) {
        return;
      }

      appState.videoPlayer.value.isPlaying = isPlaying;
      setIsVideoPlaying(isPlaying);
    };

    const handleSetPlayedSeconds = (newTime: number) => {
      if (appState.videoPlayer.value === null) {
        return;
      }

      setIsVideoCurrentTimeDifferenceLarge(false);
      appState.videoPlayer.value.currentTime = newTime;

      const currentVideoTime = videoPlayerRef.current?.getCurrentTime();
      if (currentVideoTime != null && Math.abs(currentVideoTime - newTime) > 1) {
        videoPlayerRef.current?.seekTo(newTime, "seconds");
      }

      if (currentVideoTime != null && Math.abs(currentVideoTime - newTime) > 2) {
        setIsVideoCurrentTimeDifferenceLarge(true);
      }
    };

    const handleSetVideoUrl = async (url: string) => {
      if (appState.videoPlayer.value == null) {
        return;
      }

      appState.videoPlayer.value.playlistVideo.url = url;
      setVideoUrl(url);
    };

    appHub.on(HubMessages.OnSetIsVideoPlaying, handleSetIsVideoPlaying);
    appHub.on(HubMessages.OnSetPlayedSeconds, handleSetPlayedSeconds);
    appHub.on(HubMessages.OnSetVideoUrl, handleSetVideoUrl);

    return () => {
      appHub.off(HubMessages.OnSetIsVideoPlaying);
      appHub.off(HubMessages.OnSetPlayedSeconds);
      appHub.off(HubMessages.OnSetVideoUrl);
    };
  }, [appHub.getState()]);

  const setUserVideoState = async (isPlaying: boolean) => {
    await appHub.invoke(HubMessages.SetIsVideoPlaying, appState.roomHash.value, isPlaying);
  };

  const handleStartVideo = () => {
    setUserVideoState(true);
  };

  const handlePauseVideo = () => {
    setUserVideoState(false);
  };

  const handleOnProgress = async (state: OnProgressProps) => {
    if (isVideoCurrentTimeDifferenceLarge && appState.isAdmin.value) {
      await appHub.invoke(
        HubMessages.SetPlayedSeconds,
        appState.roomHash.value,
        state.playedSeconds
      );

      setIsVideoCurrentTimeDifferenceLarge(false);
    }
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
      <div className="rounded-top-5 bg-dark bg-opacity-50 pt-4 pb-2 text-center"></div>
      <div className="d-flex justify-content-center bg-dark bg-opacity-50">
        {videoUrl !== undefined && videoUrl !== null ? (
          <ReactPlayer
            ref={videoPlayerRef}
            url={videoUrl}
            playing={isVideoPlaying}
            controls={true}
            width="100%"
            height="100%"
            style={{ objectFit: "cover", aspectRatio: `16/9` }}
            onPlay={handleStartVideo}
            onPause={handlePauseVideo}
            onProgress={handleOnProgress}
          />
        ) : (
          <div
            className="d-flex align-items-center justify-content-center text-white"
            style={{
              width: isMobileView ? "428px" : "854px",
              height: isMobileView ? "auto" : "480px",
            }}
          >
            <div className="text-center">
              <h1>
                <BsCameraVideoOffFill />
              </h1>
              <h5>No video to display</h5>
            </div>
          </div>
        )}
      </div>
      <div className="rounded-bottom-5 bg-dark bg-opacity-50 pt-2 pb-4 text-center"></div>
    </>
  );
}

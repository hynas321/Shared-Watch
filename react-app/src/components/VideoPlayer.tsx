import ReactPlayer from "react-player";
import { useContext } from "react";
import { AppStateContext } from "../context/RoomHubContext";

export default function VideoPlayer() {
  const appState = useContext(AppStateContext);

  return (
    <>
      <div className="rounded-top-5 bg-dark bg-opacity-50 pt-2 text-center">
        <span className="text-white"><b>Video player</b></span>
      </div>
      <div className="d-flex justify-content-center rounded-bottom-5 bg-dark bg-opacity-50 pt-2 pb-5">
        <ReactPlayer
          url={appState.videoPlayerState.value?.url}
          playing={appState.videoPlayerState.value?.isPlaying}
          controls={true}
          width={"854px"}
          height={"480px"}
          style={{}}
          onPlay={() => {}}
          onPause={() => {}}
        />
      </div>
    </>
  )
}

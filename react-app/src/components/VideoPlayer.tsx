import ReactPlayer from "react-player";
import { VideoPlayerSettings } from "../types/VideoPlayerSettings";

export interface VideoPlayerProps {
  videoPlayerSettings: VideoPlayerSettings
}

export default function VideoPlayer({videoPlayerSettings}: VideoPlayerProps) {
  return (
    <>
      <div className="rounded-top-5 bg-dark pt-2 text-center">
        <span className="text-white"><b>Video player</b></span>
      </div>
      <div className="d-flex justify-content-center rounded-bottom-5 bg-dark pt-2 pb-5">
        <ReactPlayer
          url={videoPlayerSettings.url}
          playing={videoPlayerSettings.isPlaying}
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

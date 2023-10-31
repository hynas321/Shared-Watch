import ControlPanel from "./ControlPanel";
import VideoPlayer from "./VideoPlayer";

export default function RoomView() {
  return (
    <div className="container">
      <div className="row">
        <div className="col-xl-8 col-lg-12 mt-2">
          <VideoPlayer
            videoName={"COSTA RICA IN 4K 60fps HDR (ULTRA HD)"}
            videoUrl={"https://www.youtube.com/watch?v=LXb3EKWsInQ"}
          />
        </div>
        <div className="col-xl-4 col-lg-12 mt-2">
          <ControlPanel />
        </div>
      </div>
    </div>
  )
}

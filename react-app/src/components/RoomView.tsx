import ControlPanel from "./ControlPanel";
import VideoPlayer from "./VideoPlayer";

export default function RoomView() {
  return (
    <>
      <div className="container">
        <div className="row">
          <div className="col-lg-8 mt-2">
            <VideoPlayer
              videoName={"REO Speedwagon - Can't Fight This Feeling (Official HD Video)"}
              videoUrl={"https://www.youtube.com/embed/zpOULjyy-n8?rel=0"}
            />
          </div>
          <div className="col-lg-4 mt-2">
            <ControlPanel />
          </div>
        </div>
      </div>
    </>
  )
}

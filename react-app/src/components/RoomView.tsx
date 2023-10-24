import Header from "./Header";
import VideoInfo from "./VideoInfo";
import SidebarMenu from "./SidebarMenu";
import VideoPlayer from "./VideoPlayer";
import RoomInfo from "./RoomInfo";

export default function RoomView() {
  return (
    <>
      <Header title="Test room" />
      <div className="container">
        <div className="row">
          <div className="col-lg-8 mt-2">
            <VideoPlayer />
            <VideoInfo videoName={"REO Speedwagon - Can't Fight This Feeling (Official HD Video)"} />
          </div>
          <div className="col-lg-4 mt-2">
            <RoomInfo />
            <SidebarMenu />
          </div>
        </div>
      </div>
    </>
  )
}

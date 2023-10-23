import SidebarMenu from "./SidebarMenu";
import VideoPlayer from "./VideoPlayer";

export default function RoomView() {
  return (
    <div className="container">
      <div className="row">
        <div className="col-lg-6">
          <VideoPlayer />
        </div>
        <div className="col-lg-6">
          <SidebarMenu />
        </div>
      </div>
    </div>
  )
}

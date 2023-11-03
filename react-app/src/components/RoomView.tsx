import { useDispatch } from "react-redux";
import ControlPanel from "./ControlPanel";
import VideoPlayer from "./VideoPlayer";
import { useEffect } from "react";
import { updatedIsInRoom } from "../redux/slices/userState-slice";

export default function RoomView() {
  const dispatch = useDispatch();

  useEffect(() => {
    dispatch(updatedIsInRoom(true));
  }, []);

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

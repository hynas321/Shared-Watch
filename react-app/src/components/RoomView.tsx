import { useDispatch } from "react-redux";
import ControlPanel from "./ControlPanel";
import VideoPlayer from "./VideoPlayer";
import { useEffect, useState } from "react";
import { updatedIsInRoom } from "../redux/slices/userState-slice";
import { HttpManager } from "../classes/HttpManager";
import { useLocation, useNavigate } from "react-router-dom";
import { useAppSelector } from "../redux/hooks";
import { ClientEndpoints } from "../classes/ClientEndpoints";

export default function RoomView() {
  const [userIsLeavingRoom, setUserIsLeavingRoom] = useState<boolean>(false);
  const userState = useAppSelector((state) => state.userState);
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const location = useLocation();
  const { roomHash, roomPassword } = location.state ?? "";

  const httpManager = new HttpManager();

  const joinRoom = async () => {
    const joinRoomOutput = await httpManager.joinRoom(roomHash, roomPassword, userState.username);

    if (joinRoomOutput === null) {
      navigate(`${ClientEndpoints.mainMenu}`);
    }

    dispatch(updatedIsInRoom(true));
  }

  const leaveRoom = async() => {
    const leaveRoomOutput = await httpManager.leaveRoom(roomHash);

    if (leaveRoomOutput) {
      setUserIsLeavingRoom(true);
    }

    navigate(`${ClientEndpoints.mainMenu}`);
  }

  useEffect(() => {
    joinRoom();
    return () => {
      if (!userIsLeavingRoom) {
        return;
      }

      leaveRoom();
    };
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
          <ControlPanel roomHash={roomHash} />
        </div>
      </div>
    </div>
  )
}

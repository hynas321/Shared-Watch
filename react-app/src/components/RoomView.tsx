import { useDispatch } from "react-redux";
import ControlPanel from "./ControlPanel";
import VideoPlayer from "./VideoPlayer";
import { useEffect } from "react";
import { updatedIsInRoom } from "../redux/slices/userState-slice";
import { HttpManager } from "../classes/HttpManager";
import { useNavigate } from "react-router-dom";
import { useAppSelector } from "../redux/hooks";
import { ClientEndpoints } from "../classes/ClientEndpoints";
import Header from "./Header";
import { toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import { PromiseOutput } from "../types/HttpTypes/PromiseOutput";
import { HttpStatusCodes } from "../classes/HttpStatusCodes";

export default function RoomView() {
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const userState = useAppSelector((state) => state.userState);
  const roomState = useAppSelector((state) => state.roomState);

  const httpManager = new HttpManager();

  const joinRoom = async () => {
    const joinRoomOutput: PromiseOutput = await httpManager.joinRoom(roomState.roomHash, roomState.password, userState.username);

    if (joinRoomOutput.status !== HttpStatusCodes.OK) {

      switch(joinRoomOutput.status) {
        case HttpStatusCodes.NOT_FOUND:
          toast.error("Room not found");
          break;
        case HttpStatusCodes.CONFLICT:
          toast.error("A user with your username already exists in the room");
          break;
        default:
          toast.error("Could not join the room");
      }

      navigate(`${ClientEndpoints.mainMenu}`);
    }

    dispatch(updatedIsInRoom(true));
  }

  const leaveRoom = async() => {
    const leaveRoomOutput = await httpManager.leaveRoom(roomState.roomHash);
    navigate(`${ClientEndpoints.mainMenu}`);
  };

  useEffect(() => {
    joinRoom();
    return () => {
      //TODO
    };
  }, []);

  return (
    <>
      <Header />
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
    </>
  )
}

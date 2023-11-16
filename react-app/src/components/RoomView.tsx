import { useDispatch } from "react-redux";
import ControlPanel from "./ControlPanel";
import VideoPlayer from "./VideoPlayer";
import { useEffect, useState } from "react";
import { updatedIsInRoom } from "../redux/slices/userState-slice";
import { HttpManager } from "../classes/HttpManager";
import { useLocation, useNavigate } from "react-router-dom";
import { useAppSelector } from "../redux/hooks";
import { ClientEndpoints } from "../classes/ClientEndpoints";
import Header from "./Header";
import { toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import { HttpStatusCodes } from "../classes/HttpStatusCodes";
import { RoomSettings } from "../types/RoomSettings";
import { VideoPlayerSettings } from "../types/VideoPlayerSettings";
import { LocalStorageManager } from "../classes/LocalStorageManager";
import { HttpUrlHelper } from "../classes/HttpUrlHelper";
import { RoomNavigationState as RoomNavigationState } from "../types/RoomNavigationState";
import { ping } from "ldrs"
import { RoomJoinOutput } from "../types/HttpTypes/Output/RoomJoinOutput";
import { animated, useSpring } from "@react-spring/web";
import { JoinRoomNavigationState } from "../types/JoinRoomNavigationState";
import { RoomTypesEnum } from "../enums/RoomTypesEnum";

export default function RoomView() {
  const [roomHash, setRoomHash] = useState<string>("");
  const [roomJoinResponse, setRoomJoinResponse] = useState<RoomJoinOutput>({
    authorizationToken: "",
    chatMessages: [],
    queuedVideos: [],
    users: [],
    roomSettings: {} as RoomSettings,
    videoPlayerSettings: {} as VideoPlayerSettings
  });
  const [isViewLoading, setIsViewLoading] = useState<boolean>(true);

  const dispatch = useDispatch();
  const navigate = useNavigate();
  const location = useLocation();
  const userState = useAppSelector((state) => state.userState);

  const { ...roomNavigationState }: RoomNavigationState = location.state ?? location.state == null;

  const httpManager = new HttpManager();
  const httpUrlHelper = new HttpUrlHelper();
  const localStorageManager = new LocalStorageManager();
  
  const checkIfRoomExists = async (hash: string) => {
    const [responseStatusCode, responseData] = await httpManager.checkIfRoomExists(hash);

    if (responseStatusCode !== HttpStatusCodes.OK) {
      navigate(`${ClientEndpoints.mainMenu}`);
    }

    const joinRoomNavigationState: JoinRoomNavigationState = {
      roomType: responseData?.roomType as RoomTypesEnum
    }

    navigate(`${ClientEndpoints.joinRoom}/${hash}`, { state: { ...joinRoomNavigationState }, replace: true });
  }

  useEffect(() => {
    ping.register();
    const hash: string = httpUrlHelper.getRoomHash(window.location.href);

    if (!hash || hash.length === 0) {
      toast.error("Room not found");
      navigate(`${ClientEndpoints.mainMenu}`);
      return;
    }

    if (!location.state) {
      checkIfRoomExists(hash);
      return;
    }
    
    setRoomHash(hash);
  }, []);

  useEffect(() => {
    if (!roomHash) {
      return;
    }

    const handleBeforeUnload = async () => {
      httpManager.leaveRoom(roomHash);
    }
  
    joinRoom();

    setTimeout(() => {
      setIsViewLoading(false);
    }, 1000);

    window.addEventListener('beforeunload', handleBeforeUnload);

    return () => {
      handleBeforeUnload();
      window.removeEventListener('beforeunload', handleBeforeUnload);
    };
  }, [roomHash]);

  const joinRoom = async () => {
    const [responseStatusCode, responseData] = await httpManager.joinRoom(roomHash, roomNavigationState.password, userState.username);

    if (responseStatusCode !== HttpStatusCodes.OK) {

      switch(responseStatusCode) {
        case HttpStatusCodes.UNAUTHORIZED:
          toast.error("Wrong room password");
          break;
        case HttpStatusCodes.FORBIDDEN:
          toast.error("Room full");
          break;
        case HttpStatusCodes.NOT_FOUND:
          toast.error("Room not found");
          break;
        case HttpStatusCodes.CONFLICT:
          toast.error("The user is already in the room");
          break;
        default:
          toast.error("Could not join the room");
      }

      navigate(`${ClientEndpoints.mainMenu}`);
      return;
    }

    localStorageManager.setAuthorizationToken(responseData?.authorizationToken as string);
    dispatch(updatedIsInRoom(true));

    setRoomJoinResponse(responseData as RoomJoinOutput);
  }

  const springs = useSpring({
    from: { y: 200 },
    to: { y: 0 },
    config: {
      mass: 1,
      tension: 250,
      friction:15
    },
    delay: 1000
  })

  return (
    <>
      <Header />
      <div className="container">
        <div className="row">
          {
            isViewLoading &&
            <div className="d-flex align-items-center justify-content-center" style={{ height: "70vh" }}>
            <l-ping
              size="250"
              speed="1.5" 
              color="white" 
            ></l-ping>
            </div>
          }
          {
            !isViewLoading &&
            <>
              <animated.div style={{...springs}} className="col-xl-8 col-lg-12 mt-2">
                <VideoPlayer videoPlayerSettings={{url: "abc", isPlaying: false}}/>
              </animated.div>
              <animated.div style={{...springs}} className="col-xl-4 col-lg-12 mt-2">
                <ControlPanel
                  initialChatMessages={roomJoinResponse.chatMessages}
                  initialQueuedVideos={roomJoinResponse.queuedVideos}
                  initialUsers={roomJoinResponse.users}
                  initialRoomSettings={roomJoinResponse.roomSettings}
                />
              </animated.div>
            </>
          }
        </div>
      </div>
    </>
  )
}

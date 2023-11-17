import { useDispatch } from "react-redux";
import ControlPanel from "./ControlPanel";
import VideoPlayer from "./VideoPlayer";
import { useContext, useEffect, useState } from "react";
import { HttpManager } from "../classes/HttpManager";
import { useLocation, useNavigate } from "react-router-dom";
import { ClientEndpoints } from "../classes/ClientEndpoints";
import Header from "./Header";
import { toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import { HttpStatusCodes } from "../classes/HttpStatusCodes";
import { HttpUrlHelper } from "../classes/HttpUrlHelper";
import { ping } from "ldrs"
import { animated, useSpring } from "@react-spring/web";
import { JoinRoomNavigationState } from "../types/JoinRoomNavigationState";
import { RoomTypesEnum } from "../enums/RoomTypesEnum";
import { updatedIsInRoom } from "../redux/slices/userState-slice";
import { RoomHubContext } from "../context/RoomHubContext";
import * as signalR from "@microsoft/signalr";
import { HubEvents } from "../classes/HubEvents";
import { User } from "../types/User";

export default function RoomView() {  
  const [roomHash, setRoomHash] = useState<string>("");
  const [isViewLoading, setIsViewLoading] = useState<boolean>(false);

  const roomHub = useContext(RoomHubContext);
  const dispatch = useDispatch();
  const navigate = useNavigate();
  const location = useLocation();

  const { roomNavigationState, roomInformation } = location.state ?? location.state === null;

  const httpManager = new HttpManager();
  const httpUrlHelper = new HttpUrlHelper();
  
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
    console.log("hello");
    dispatch(updatedIsInRoom(true));
    ping.register();
  
    const hash: string = httpUrlHelper.getRoomHash(window.location.href);

    if (!hash || hash.length === 0) {
      toast.error("Room not found");
      navigate(`${ClientEndpoints.mainMenu}`);
      return;
    }

    if (!roomNavigationState || !roomInformation) {
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

    //setTimeout(() => {
      //setIsViewLoading(false);
    //}, 1000);

    window.addEventListener('beforeunload', handleBeforeUnload);

    return () => {
      handleBeforeUnload();
      window.removeEventListener('beforeunload', handleBeforeUnload);
    };
  }, [roomHash]);

  useEffect(() => {
    if (roomHub.getState() !== signalR.HubConnectionState.Connected) {
      return;
    }

    roomHub.on(HubEvents.OnJoinRoom, (newUser: User) => {
      console.log(newUser);
    });

    roomHub.on(HubEvents.OnLeaveRoom, (removedUser: User) => {
      console.log(removedUser);
    });

    return () => {
      roomHub.off(HubEvents.OnJoinRoom);
      roomHub.off(HubEvents.OnLeaveRoom);
    }
    }, [roomHub.getState()]);


  const springs = useSpring({
    from: { y: 200 },
    to: { y: 0 },
    config: {
      mass: 1,
      tension: 250,
      friction:15
    },
    //delay: 1000
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
                  initialChatMessages={roomInformation.chatMessages ?? []}
                  initialQueuedVideos={roomInformation.queuedVideos ?? []}
                  initialUsers={roomInformation.users ?? []}
                  initialRoomSettings={roomInformation.roomSettings ?? {}}
                />
              </animated.div>
            </>
          }
        </div>
      </div>
    </>
  )
}

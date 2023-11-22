import ControlPanel from "./ControlPanel";
import VideoPlayer from "./VideoPlayer";
import { useContext, useEffect, useState } from "react";
import { HttpManager } from "../classes/HttpManager";
import { useNavigate } from "react-router-dom";
import { ClientEndpoints } from "../classes/ClientEndpoints";
import Header from "./Header";
import { toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import { HttpStatusCodes } from "../classes/HttpStatusCodes";
import { HttpUrlHelper } from "../classes/HttpUrlHelper";
import { ping } from "ldrs"
import { animated, useSpring } from "@react-spring/web";
import { AppStateContext } from "../context/RoomHubContext";
import { RoomTypesEnum } from "../enums/RoomTypesEnum";

export default function RoomView() {
  const appState = useContext(AppStateContext);
  const navigate = useNavigate();

  const httpManager = new HttpManager();
  const httpUrlHelper = new HttpUrlHelper();
  
  const checkIfRoomExists = async (hash: string) => {
    const [responseStatusCode, responseData] = await httpManager.checkIfRoomExists(hash);

    if (responseStatusCode !== HttpStatusCodes.OK) {
      navigate(`${ClientEndpoints.mainMenu}`, { replace: true });
    }
    
    appState.roomType.value = responseData?.roomType as RoomTypesEnum;

    navigate(`${ClientEndpoints.joinRoom}/${hash}`, { replace: true });
  }

  useEffect(() => {
    appState.isInRoom.value = true;
    ping.register();
  
    const hash: string = httpUrlHelper.getRoomHash(window.location.href);

    if (!hash || hash.length === 0) {
      toast.error("Room not found");
      navigate(`${ClientEndpoints.mainMenu}`, { replace: true });
      return;
    }

    //if (!roomNavigationState || !roomInformation) {
      //checkIfRoomExists(hash);
      //return;
    //}

    appState.roomHash.value = hash;
  }, []);

  useEffect(() => {
    if (!appState.roomHash.value) {
      return;
    }

    const handleBeforeUnload = async () => {
      httpManager.leaveRoom(appState.roomHash.value);
    }

    window.addEventListener('beforeunload', handleBeforeUnload);

    return () => {
      handleBeforeUnload();
      window.removeEventListener('beforeunload', handleBeforeUnload);
    };
  }, [appState.roomHash.value]);

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
          <animated.div style={{ ...springs }} className="col-xl-8 col-lg-12 col-xs-12 mt-2">
            <VideoPlayer />
          </animated.div>
          <animated.div style={{ ...springs }} className="col-xl-4 col-lg-12 col-xs-6 mt-2">
            <ControlPanel />
          </animated.div>
        </div>
      </div>
    </>
  )
}

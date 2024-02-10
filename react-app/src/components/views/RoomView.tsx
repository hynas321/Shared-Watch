import ControlPanel from "../ControlPanel";
import VideoPlayer from "../VideoPlayer";
import { useContext, useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { ClientEndpoints } from "../../classes/ClientEndpoints";
import Header from "../Header";
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import { HttpUrlHelper } from "../../classes/HttpUrlHelper";
import { animated, useSpring } from "@react-spring/web";
import { AppStateContext, appHub } from "../../context/AppContext";
import { PanelsEnum } from "../../enums/PanelsEnum";
import { useSignal } from "@preact/signals-react";
import { ToastNotificationEnum } from "../../enums/ToastNotificationEnum";
import { ping } from 'ldrs'

export default function RoomView() {
  const appState = useContext(AppStateContext);
  const navigate = useNavigate();

  const isRoomLoading = useSignal<boolean>(true);

  const httpUrlHelper = new HttpUrlHelper();

  useEffect(() => {
    ping.register();
    appState.isInRoom.value = true;
  
    const hash: string = httpUrlHelper.getRoomHash(window.location.href);

    if (!hash || hash.length === 0) {
      toast.error(
        "Room not found", {
          containerId: ToastNotificationEnum.Main
        }
      );
      navigate(`${ClientEndpoints.mainMenu}`);
      return;
    }

    if (!appState.joinedViaView.value) {
      appState.roomHash.value = hash;
      navigate(`${ClientEndpoints.joinRoom}/${hash}`, { replace: true });
      return;
    }

    return () => {
      appState.activePanel.value = PanelsEnum.Chat;
    }
  }, []);

  useEffect(() => {
    if (!appState.roomHash.value || !appState.joinedViaView.value) {
      return;
    }

    setTimeout(() => {
      isRoomLoading.value = false;
    }, 800)

    const handleBeforeUnload = async () => {
      //await httpManager.leaveRoom(appState.roomHash.value);
      navigate(`${ClientEndpoints.mainMenu}`, { replace: true });
      return;
    }

    appHub.onclose(() => {
      appState.connectionId.value = null;
    })

    window.addEventListener('beforeunload', handleBeforeUnload);

    return () => {
      handleBeforeUnload();
      window.removeEventListener('beforeunload', handleBeforeUnload);
    };
  }, [appState.roomHash.value]);

  const springs = useSpring({
    from: { y: 400 },
    to: { y: 0 },
    config: {
      mass: 1.75,
      tension: 150,
      friction: 20,
    }
  });

  return (
    <>
      {
        isRoomLoading.value ?
        <div className="container-fluid d-flex justify-content-center align-items-center vh-100">
          <div className="col-md-6 text-center">
            <l-ping
              size="100"
              speed="1.25" 
              color="white" 
            ></l-ping>
            <h5 className="text-white">Joining the room...</h5>
          </div>
        </div>
        :
        <>
          <Header />
          <ToastContainer
            containerId={ToastNotificationEnum.Room}
            position="top-right"
            autoClose={1500}
            hideProgressBar={true}
            closeOnClick={true}
            draggable={true}
            pauseOnHover={false}
            theme="dark"
            style={{top: '0px', opacity: 0.9}}
            limit={1}
          />
          <div className=" container-lg">
            <div className="row">
              <animated.div style={{ ...springs }} className="col-xl-8 col-lg-12 col-xs-12 mt-3 mb-3">
                <VideoPlayer />
              </animated.div>
              <animated.div style={{ ...springs }} className="col-xl-4 col-lg-8 mx-lg-auto mt-3 mb-3">
                <ControlPanel />
              </animated.div>
            </div>
          </div>
        </>
      }
    </>
  )
}

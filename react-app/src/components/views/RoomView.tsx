import ControlPanel from "../ControlPanel";
import VideoPlayer from "../VideoPlayer";
import { useContext, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { ClientEndpoints } from "../../classes/ClientEndpoints";
import Header from "../Header";
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import { HttpUrlHelper } from "../../classes/HttpUrlHelper";
import { animated, useSpring } from "@react-spring/web";
import { AppStateContext, roomHub } from "../../context/RoomHubContext";
import { PanelsEnum } from "../../enums/PanelsEnum";
import { useSignal } from "@preact/signals-react";
import { ToastNotificationEnum } from "../../enums/ToastNotificationEnum";

export default function RoomView() {
  const appState = useContext(AppStateContext);
  const navigate = useNavigate();

  const isContentVisible = useSignal<boolean>(false);

  const httpUrlHelper = new HttpUrlHelper();

  useEffect(() => {
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
      navigate(`${ClientEndpoints.joinRoom}/${hash}`);
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

    const handleBeforeUnload = async () => {
      //await httpManager.leaveRoom(appState.roomHash.value);
      navigate(`${ClientEndpoints.mainMenu}`);
      return;
    }

    roomHub.onclose(() => {
      appState.connectionIssue.value = true;
    })

    window.addEventListener('beforeunload', handleBeforeUnload);

    isContentVisible.value = true;

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
      {
        isContentVisible &&
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
          <div className="container-fluid-md container-lg">
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

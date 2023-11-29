import ControlPanel from "./ControlPanel";
import VideoPlayer from "./VideoPlayer";
import { useContext, useEffect } from "react";
import { HttpManager } from "../classes/HttpManager";
import { useNavigate } from "react-router-dom";
import { ClientEndpoints } from "../classes/ClientEndpoints";
import Header from "./Header";
import { toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import { HttpUrlHelper } from "../classes/HttpUrlHelper";
import { ping } from "ldrs"
import { animated, useSpring } from "@react-spring/web";
import { AppStateContext, roomHub } from "../context/RoomHubContext";
import { PanelsEnum } from "../enums/PanelsEnum";
import { useSignal } from "@preact/signals-react";

export default function RoomView() {
  const appState = useContext(AppStateContext);
  const navigate = useNavigate();

  const isContentVisible = useSignal<boolean>(false);

  const httpManager = new HttpManager();
  const httpUrlHelper = new HttpUrlHelper();

  useEffect(() => {
    appState.isInRoom.value = true;
    ping.register();
  
    const hash: string = httpUrlHelper.getRoomHash(window.location.href);

    if (!hash || hash.length === 0) {
      toast.error("Room not found");
      navigate(`${ClientEndpoints.mainMenu}`, { replace: true });
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

    const handleBeforeUnload = async () => {
      httpManager.leaveRoom(appState.roomHash.value);
      navigate(`${ClientEndpoints.mainMenu}`, { replace: true });
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
          <div className="container-fluid-md container-lg">
            <div className="row">
              <animated.div style={{ ...springs }} className="col-xl-8 col-lg-12 col-xs-12 mt-2 mb-3">
                <VideoPlayer />
              </animated.div>
              <animated.div style={{ ...springs }} className="col-xl-4 col-lg-12 mt-2 mb-3">
                <ControlPanel />
              </animated.div>
            </div>
          </div>
        </>
      }
    </>
  )
}

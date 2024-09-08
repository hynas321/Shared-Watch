import ControlPanel from "../ControlPanel";
import VideoPlayer from "../VideoPlayer";
import { useContext, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { ClientEndpoints } from "../../classes/constants/ClientEndpoints";
import Header from "../Header";
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import { HttpUrlHelper } from "../../classes/helpers/HttpUrlHelper";
import { animated, useSpring } from "@react-spring/web";
import { AppStateContext, appHub } from "../../context/AppContext";
import { PanelsEnum } from "../../enums/PanelsEnum";
import { ToastNotificationEnum } from "../../enums/ToastNotificationEnum";

export default function RoomView() {
  const appState = useContext(AppStateContext);
  const navigate = useNavigate();

  const httpUrlHelper = new HttpUrlHelper();

  useEffect(() => {
    appState.isInRoom.value = true;
  
    const hash = httpUrlHelper.getRoomHash(window.location.href);

    if (!hash || hash.length === 0) {
      toast.error("Room not found", {
        containerId: ToastNotificationEnum.Main
      });
      navigate(ClientEndpoints.mainMenu);
      return;
    }

    if (!appState.joinedViaView.value) {
      appState.roomHash.value = hash;
      navigate(`${ClientEndpoints.joinRoom}/${hash}`, { replace: true });
      return;
    }

    return () => {
      appState.activePanel.value = PanelsEnum.Chat;
    };
  }, []);

  useEffect(() => {
    if (!appState.roomHash.value || !appState.joinedViaView.value) {
      return;
    }

    const handleBeforeUnload = async () => {
      navigate(ClientEndpoints.mainMenu, { replace: true });
      return;
    };

    appHub.onclose(() => {
      appState.connectionId.value = null;
    });

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
        style={{ top: '0px', opacity: 0.9 }}
        limit={1}
      />
      <div className="container-lg">
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
  );
}

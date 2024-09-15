import { useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { ToastContainer, toast } from "react-toastify";
import { animated, useSpring } from "@react-spring/web";
import "react-toastify/dist/ReactToastify.css";
import Header from "../Header";
import VideoPlayer from "../VideoPlayer";
import ControlPanel from "../ControlPanel";
import { HttpUrlHelper } from "../../classes/helpers/HttpUrlHelper";
import { ClientEndpoints } from "../../classes/constants/ClientEndpoints";
import { appHub, appState } from "../../context/AppContext";
import { PanelsEnum } from "../../enums/PanelsEnum";
import { ToastNotificationEnum } from "../../enums/ToastNotificationEnum";
import { SessionStorageService } from "../../classes/services/SessionStorageService";
import { HubMessages } from "../../classes/constants/HubMessages";
import { jwtDecode } from "jwt-decode";

export default function RoomView() {
  const navigate = useNavigate();
  const httpUrlHelper = new HttpUrlHelper();
  const sessionStorageService = SessionStorageService.getInstance();

  const springs = useSpring({
    from: { y: 400 },
    to: { y: 0 },
    config: { mass: 1.75, tension: 150, friction: 20 },
  });

  useEffect(() => {
    const setupRoomView = async () => {
      await appHub.start();

      appHub.on(HubMessages.OnReceiveJwt, (jwt: string) => {
        sessionStorageService.setAuthorizationToken(jwt);

        const decodedJwt = jwtDecode<CustomJwtPayload>(jwt);
        const role = decodedJwt["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];

        appState.isAdmin.value = role == "Admin";
      });
    };

    setupRoomView();

    return () => {
      appHub.off(HubMessages.OnReceiveJwt);
      appHub.stop();
      sessionStorageService.clearAuthorizationToken();
      appState.isInRoom.value = false;
    };
  }, []);

  useEffect(() => {
    appState.isInRoom.value = true;

    const roomHash = httpUrlHelper.getRoomHash(window.location.href);

    if (!roomHash) {
      toast.error("Room not found", {
        containerId: ToastNotificationEnum.Main,
      });
      navigate(ClientEndpoints.mainMenu);
      return;
    }

    if (!appState.joinedViaView.value) {
      appState.roomHash.value = roomHash;
      navigate(`${ClientEndpoints.joinRoom}/${roomHash}`, { replace: true });
      return;
    }

    return () => {
      appState.activePanel.value = PanelsEnum.Chat;
    };
  }, [appState, navigate, httpUrlHelper]);

  return (
    <>
      <Header />
      <ToastContainer
        containerId={ToastNotificationEnum.Room}
        position="top-right"
        autoClose={1500}
        hideProgressBar
        closeOnClick
        draggable
        pauseOnHover={false}
        theme="dark"
        style={{ top: "0px", opacity: 0.9 }}
        limit={1}
      />
      <div className="container-lg">
        <div className="row">
          <animated.div style={springs} className="col-xl-8 col-lg-12 col-xs-12 mt-3 mb-3">
            <VideoPlayer />
          </animated.div>
          <animated.div style={springs} className="col-xl-4 col-lg-8 mx-lg-auto mt-3 mb-3">
            <ControlPanel />
          </animated.div>
        </div>
      </div>
    </>
  );
}

import { CSSProperties, useContext, useEffect, useLayoutEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { ToastContainer, toast } from "react-toastify";
import { animated, useSpring } from "@react-spring/web";
import "react-toastify/dist/ReactToastify.css";
import Header from "../Header";
import VideoPlayer from "../video-player/VideoPlayer";
import ControlPanel from "../ControlPanel";
import { getRoomHash } from "../../classes/helpers/HttpUrlHelper";
import { ClientEndpoints } from "../../classes/constants/ClientEndpoints";
import { AppHubContext, appState } from "../../context/AppContext";
import { PanelsEnum } from "../../enums/PanelsEnum";
import { ToastNotificationEnum } from "../../enums/ToastNotificationEnum";
import { HubMessages } from "../../classes/constants/HubMessages";
import * as signalR from "@microsoft/signalr";
import BackgroundSelector from "../shared/BackgroundSelector";
import api from "../../classes/Http/Api";
import { HttpStatusCode } from "axios";

const PRESENCE_REFRESH_INTERVAL_MS = 15_000;
const CONNECTION_RECOVERY_WINDOW_MS = 15_000;
const MAX_CONSECUTIVE_REQUEST_FAILURES = 3;
const REQUEST_FAILURE_WINDOW_MS = 15_000;

export default function RoomView() {
  const navigate = useNavigate();
  const appHub = useContext(AppHubContext);
  const videoShellRef = useRef<HTMLDivElement>(null);
  const connectionRecoveryTimer = useRef<number | undefined>(undefined);
  const hasEstablishedConnection = useRef(false);
  const [videoHeight, setVideoHeight] = useState<number | null>(null);

  const springs = useSpring({
    from: { y: 180, opacity: 0 },
    to: { y: 0, opacity: 1 },
    config: { mass: 1.2, tension: 180, friction: 18 },
  });

  useLayoutEffect(() => {
    const videoShell = videoShellRef.current;
    if (!videoShell) {
      return;
    }

    const updateVideoHeight = () => {
      const measuredHeight = videoShell.getBoundingClientRect().height;
      if (measuredHeight > 0) {
        setVideoHeight(measuredHeight);
      }
    };

    updateVideoHeight();
    const observer = new ResizeObserver(updateVideoHeight);
    observer.observe(videoShell);
    return () => observer.disconnect();
  }, []);

  useEffect(() => {
    let isActive = true;
    let requestFailureCount = 0;
    let requestFailureTimer: number | undefined;

    const returnToMainMenuAfterDisconnect = () => {
      if (!isActive) {
        return;
      }

      isActive = false;
      sessionStorage.setItem(
        "backendDisconnectMessage",
        "The connection to the server has been lost."
      );
      appState.isInRoom.value = false;
      void appHub.stop().catch((error: unknown) => {
        console.warn("Unable to close the disconnected room connection.", error);
      });
      navigate(ClientEndpoints.mainMenu, { replace: true });
    };

    const clearConnectionRecovery = () => {
      if (connectionRecoveryTimer.current !== undefined) {
        window.clearTimeout(connectionRecoveryTimer.current);
        connectionRecoveryTimer.current = undefined;
      }
      toast.dismiss("room-connection-recovery");
    };

    const handleConnectionStateChanged = () => {
      if (!isActive) {
        return;
      }

      const state = appHub.getState();
      if (state === signalR.HubConnectionState.Connected) {
        hasEstablishedConnection.current = true;
        clearConnectionRecovery();
        return;
      }

      if (!hasEstablishedConnection.current || connectionRecoveryTimer.current !== undefined) {
        return;
      }

      toast.info("Connection lost. Trying to reconnect…", {
        containerId: ToastNotificationEnum.Room,
        toastId: "room-connection-recovery",
        autoClose: false,
        closeOnClick: false,
        draggable: false,
      });
      connectionRecoveryTimer.current = window.setTimeout(() => {
        connectionRecoveryTimer.current = undefined;
        toast.dismiss("room-connection-recovery");
        returnToMainMenuAfterDisconnect();
      }, CONNECTION_RECOVERY_WINDOW_MS);
    };

    let healthCheckInProgress = false;

    const handleRequestFailure = async () => {
      if (!isActive) {
        return;
      }

      requestFailureCount += 1;
      if (requestFailureTimer === undefined) {
        requestFailureTimer = window.setTimeout(() => {
          requestFailureCount = 0;
          requestFailureTimer = undefined;
        }, REQUEST_FAILURE_WINDOW_MS);
      }

      if (requestFailureCount >= MAX_CONSECUTIVE_REQUEST_FAILURES && !healthCheckInProgress) {
        healthCheckInProgress = true;
        const healthStatus = await api.healthCheck();
        healthCheckInProgress = false;
        if (!isActive) {
          return;
        }

        if (healthStatus !== HttpStatusCode.Ok) {
          returnToMainMenuAfterDisconnect();
        } else {
          requestFailureCount = 0;
        }
      }
    };

    const unsubscribeRequestFailures = appHub.onRequestFailed(handleRequestFailure);
    const handleJwtUpdate = async () => {
      const refreshStatus = await api.refreshAuthorizationToken();
      if (refreshStatus < 200 || refreshStatus >= 300) {
        if (isActive) {
          console.warn("Unable to refresh the room connection after a role change.");
        }
      }
    };

    const setupRoomView = async () => {
      try {
        await appHub.start();
        if (!isActive) {
          await appHub.stop();
          return;
        }

        appHub.on(HubMessages.OnReceiveJwt, handleJwtUpdate);
      } catch (error) {
        if (isActive) {
          handleRequestFailure();
          console.warn("Unable to connect to the room.", error);
        }
      }
    };

    void setupRoomView();

    const unsubscribeConnectionState = appHub.onStateChanged(handleConnectionStateChanged);
    handleConnectionStateChanged();

    const presenceInterval = window.setInterval(() => {
      if (appHub.getState() === signalR.HubConnectionState.Connected) {
        void appHub.invoke(HubMessages.RefreshConnectionPresence, appState.roomHash.value)
          .catch((error: unknown) => console.warn("Unable to refresh room presence.", error));
      }
    }, PRESENCE_REFRESH_INTERVAL_MS);

    return () => {
      isActive = false;
      unsubscribeRequestFailures();
      if (requestFailureTimer !== undefined) {
        window.clearTimeout(requestFailureTimer);
      }
      window.clearInterval(presenceInterval);
      unsubscribeConnectionState();
      clearConnectionRecovery();
      appHub.off(HubMessages.OnReceiveJwt, handleJwtUpdate);
      void appHub.stop().catch((error: unknown) => {
        console.warn("Unable to close the room connection.", error);
      });
      appState.isInRoom.value = false;
    };
  }, [appHub, navigate]);

  useEffect(() => {
    appState.isInRoom.value = true;
    appState.activePanel.value = PanelsEnum.Chat;

    const roomHash = getRoomHash(window.location.href);

    if (!roomHash) {
      toast.error("Room not found", {
        containerId: ToastNotificationEnum.Main,
      });
      navigate(ClientEndpoints.mainMenu);
      return;
    }

    if (!appState.roomName.value) {
      void api.getRoom(roomHash).then(([status, room]) => {
        if (status === HttpStatusCode.Ok && room?.roomName) {
          appState.roomName.value = room.roomName;
        }
      });
    }

    if (!appState.joinedViaView.value) {
      appState.roomHash.value = roomHash;
      navigate(`${ClientEndpoints.joinRoom}/${roomHash}`, { replace: true });
      return;
    }

    return undefined;
  }, [navigate]);

  return (
    <>
      <div className="container-lg room-navbar-container">
        <Header />
      </div>
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
        <div
          className="row room-layout-row"
          style={videoHeight ? ({ "--room-video-height": `${videoHeight}px` } as CSSProperties) : undefined}
        >
          <animated.div style={springs} className="room-video-column col-xl-8 col-lg-12 col-xs-12 mt-3 mb-3">
            <div ref={videoShellRef} className="room-video-player-shell">
              <VideoPlayer />
            </div>
          </animated.div>
          <animated.div
            style={springs}
            className="room-control-column col-xl-4 col-lg-8 mx-lg-auto mt-3 mb-3"
          >
            <ControlPanel />
          </animated.div>
        </div>
        <div className="room-background-selector">
          <BackgroundSelector />
        </div>
      </div>
    </>
  );
}

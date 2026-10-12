import {
  BsDoorOpenFill,
  BsFillCameraReelsFill,
  BsFillPersonFill,
  BsShieldFillCheck,
} from "react-icons/bs";
import { AppHubContext, AppStateContext } from "../context/AppContext";
import { useEffect, useContext, useState } from "react";
import useClipboardApi from "use-clipboard-api";
import { useSignal } from "@preact/signals-react";
import { BsFillLayersFill } from "react-icons/bs";
import { toast } from "react-toastify";
import { ToastNotificationEnum } from "../enums/ToastNotificationEnum";
import { useNavigate } from "react-router-dom";
import { getRoomHash } from "../classes/helpers/HttpUrlHelper";
import { ClientEndpoints } from "../classes/constants/ClientEndpoints";
import Button from "./shared/Button";
import api from "../classes/Http/Api";
import { useSessionStorage } from "../hooks/useSessionStorage";
import * as signalR from "@microsoft/signalr";

export default function Header() {
  const appState = useContext(AppStateContext);
  const appHub = useContext(AppHubContext);

  const navigate = useNavigate();
  const [, copy] = useClipboardApi();
  const { username } = useSessionStorage()

  const buttonColor = useSignal<string>("primary");
  const [connectionState, setConnectionState] = useState(appHub.getState());

  useEffect(() => {
    const refreshConnectionState = () => setConnectionState(appHub.getState());
    const unsubscribe = appHub.onStateChanged(refreshConnectionState);
    refreshConnectionState();
    return unsubscribe;
  }, [appHub]);

  const isConnected = connectionState === signalR.HubConnectionState.Connected;
  const isReconnecting = connectionState === signalR.HubConnectionState.Reconnecting;

  useEffect(() => {
    appState.username.value = username;
    appState.roomHash.value = getRoomHash(window.location.href);
  }, [appState, username]);

  const handleLeaveRoomButtonClick = async () => {
    try {
      await api.leaveRoom(appState.roomHash.value);
    } finally {
      appState.isInRoom.value = false;
      await appHub.stop();
      navigate(ClientEndpoints.mainMenu, { replace: true });
    }
  };

  const handleCopyToClipboard = () => {
    const clipboardValue = window.location.href.replace("room", "joinRoom");

    copy(clipboardValue);
    buttonColor.value = "success";

    toast.success("Invitation URL copied to clipboard", {
      containerId: ToastNotificationEnum.Room,
    });

    setTimeout(() => {
      buttonColor.value = "primary";
    }, 500);
  };

  return (
    <div>
      <nav className="navbar navbar-dark app-navbar mb-2 mt-2">
        <div className="app-navbar-main d-flex align-items-center">
          <div className="navbar-brand app-navbar-brand">
            <i>
              <b>Shared Watch</b>{" "}
            </i>
            <BsFillCameraReelsFill />
          </div>
          <div className="text-white app-navbar-user">
            {appState.isAdmin.value ? <BsShieldFillCheck /> : <BsFillPersonFill />}
            <span className="text-break text-orange ms-2">
              <b>{appState.username.value}</b>
            </span>
          </div>
          <span
            className={`connection-status ${isConnected ? "connection-status-connected" : "connection-status-warning"}`}
            role="status"
            title={isConnected ? "Connected to the shared room" : "The room connection is recovering"}
          >
            <span className="connection-status-dot" aria-hidden="true" />
            {isConnected ? "Connected" : isReconnecting ? "Reconnecting…" : "Offline"}
          </span>
        </div>
        <div className="d-flex justify-content-end">
          <div className="app-navbar-actions justify-content-end mt-header">
            <Button
              text={<><BsFillLayersFill /> Invitation Link</>}
              classNames={`btn btn-${buttonColor.value} btn-sm`}
              onClick={handleCopyToClipboard}
            />
            <Button
              text={<><BsDoorOpenFill /> Leave Room</>}
              classNames={"btn btn-danger btn-sm"}
              onClick={handleLeaveRoomButtonClick}
            />
          </div>
        </div>
      </nav>
    </div>
  );
}

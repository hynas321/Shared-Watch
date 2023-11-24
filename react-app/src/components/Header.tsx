import { BsDoorOpenFill, BsFillCameraReelsFill } from "react-icons/bs";
import { InputForm } from "./InputForm";
import Button from "./Button";
import { useNavigate } from "react-router-dom";
import { ClientEndpoints } from "../classes/ClientEndpoints";
import { LocalStorageManager } from "../classes/LocalStorageManager";
import { HttpUrlHelper } from "../classes/HttpUrlHelper";
import { AppStateContext, RoomHubContext } from "../context/RoomHubContext";
import { useEffect, useContext } from "react";
import { HubEvents } from "../classes/HubEvents";
import * as signalR from "@microsoft/signalr";
import { toast } from "react-toastify";
import { HttpManager } from "../classes/HttpManager";
import useClipboardApi from "use-clipboard-api";
import { useSignal } from "@preact/signals-react";

export default function Header() {
  const appState = useContext(AppStateContext);
  const roomHub = useContext(RoomHubContext);

  const navigate = useNavigate();
  const [, copy] = useClipboardApi();

  const buttonColor = useSignal<string>("primary");
  
  const httpManager = new HttpManager();
  const localStorageManager = new LocalStorageManager();
  const httpUrlHelper = new HttpUrlHelper();

  useEffect(() => {
    appState.username.value = localStorageManager.getUsername();
    appState.roomHash.value = httpUrlHelper.getRoomHash(window.location.href);
  }, []);

  useEffect(() => {
    const startRoomHubConnection = async () => {
      try {
        roomHub.on(HubEvents.OnReceiveConnectionId, (connectionId: string) => {
          appState.connectionId.value = connectionId;
        });

        await roomHub.start().then(() => {
          appState.connectionId.value = roomHub.getConnection().connectionId;
        });

      } catch (error) {
        toast.error("Could not establish a connection, please refresh the page");
      }
    };
  
    if (roomHub.getState() !== signalR.HubConnectionState.Connected) {
      startRoomHubConnection();
    }
  }, [roomHub]);

  const handleLeaveRoomButtonClick = async () => {
    httpManager.leaveRoom(appState.roomHash.value);

    appState.isInRoom.value = false;
    navigate(ClientEndpoints.mainMenu);
  }

  const handleCopyToClipboard = async () => {
    const clipboardValue = window.location.href.replace("room", "joinRoom");

    copy(clipboardValue);
    buttonColor.value = "success";

    setTimeout(() => {
      buttonColor.value = "primary";
    }, 500);
  }

  return (
    <nav className="navbar navbar-dark mb-3">
      <div className="d-flex align-items-center justify-content-center">
        <a className="navbar-brand ms-3" href="/"><i><b>SharedWatch</b></i> <BsFillCameraReelsFill /></a>
        {!appState.isInRoom.value &&
          <InputForm
            classNames={`form-control form-control-sm rounded-3 ms-3 ${appState.username.value.length < 3 ? "is-invalid" : "is-valid"}`}
            placeholder={"Username (min. 3 chars)"}
            value={appState.username.value}
            trim={true}
            isEnabled={true}
            onChange={(value: string) => {
              appState.username.value = value;
              localStorageManager.setUsername(value);
            }}
          />
        }
        {appState.isInRoom.value &&
          <span className="text-white ms-3">Your username: <b>{appState.username.value}</b></span>
        }
      </div>
      {
        appState.isInRoom.value &&
        <>
          <div className="djustify-content-end me-3">
            <Button
              text={<><BsDoorOpenFill /> Copy URL</>}
              classNames={`btn btn-${buttonColor.value} btn-sm me-4 ms-3`}
              onClick={handleCopyToClipboard}
            />
            <Button
              text={<><BsDoorOpenFill /> Leave room</>}
              classNames={"btn btn-danger btn-sm"}
              onClick={handleLeaveRoomButtonClick}
            />
          </div>
        </>
      }
    </nav>
  )
}
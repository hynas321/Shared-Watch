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
import { HttpManager } from "../classes/HttpManager";
import useClipboardApi from "use-clipboard-api";
import { useSignal } from "@preact/signals-react";
import { BsEmojiFrownFill } from "react-icons/bs";
import { BsFilePerson } from "react-icons/bs";
import { BsFillLayersFill } from "react-icons/bs";


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
          appState.connectionIssue.value = false;
        });

      } catch (error) {
        appState.connectionIssue.value = true;
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
    <div>
      <nav className="navbar navbar-dark mb-2 mt-2">
        <div className="d-flex align-items-center">
          <a className="navbar-brand ms-3" href="/"><i><b>Shared Watch</b></i> <BsFillCameraReelsFill /></a>
          {
            (!appState.isInRoom.value && appState.connectionIssue.value === false) &&
            <div className="ms-3 me-3">
              <InputForm
                classNames={`form-control form-control rounded-3  ${appState.username.value.length < 3 ? "is-invalid" : "is-valid"}`}
                placeholder={"Username (min. 3 chars)"}
                value={appState.username.value}
                trim={true}
                isEnabled={true}
                onChange={(value: string) => {
                  appState.username.value = value;
                  localStorageManager.setUsername(value);
                }}
              />
            </div>
          }
          {
            (appState.isInRoom.value && appState.connectionIssue.value === false) &&
            <div className="text-white ms-3 me-3"><BsFilePerson /><span className="text-break text-warning ms-2"><b>{appState.username.value}</b></span></div>
          }
          {
            (appState.connectionIssue.value === true) &&
            <span className="text-warning ms-3 me-3"><b>CONNECTION ISSUE <BsEmojiFrownFill/></b></span>
          }
        </div>
        <div className="d-flex justify-content-end">
          {
            appState.isInRoom.value &&
            <div className="justify-content-end me-3 mt-header">
              <Button
                text={<><BsFillLayersFill /> Copy URL</>}
                classNames={`btn btn-${buttonColor.value} btn-sm me-4 ms-3`}
                onClick={handleCopyToClipboard}
              />
              <Button
                text={<><BsDoorOpenFill /> Leave room</>}
                classNames={"btn btn-danger btn-sm"}
                onClick={handleLeaveRoomButtonClick}
              />
            </div>
          }
        </div>
      </nav>
    </div>
  );
}
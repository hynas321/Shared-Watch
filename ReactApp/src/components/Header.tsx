import { BsDoorOpenFill, BsFillCameraReelsFill, BsFillPersonFill, BsShieldFillCheck } from "react-icons/bs";
import { InputField } from "./InputField";
import Button from "./Button";
import { useNavigate } from "react-router-dom";
import { ClientEndpoints } from "../classes/constants/ClientEndpoints";
import { LocalStorageService } from "../classes/services/LocalStorageService";
import { HttpUrlHelper } from "../classes/helpers/HttpUrlHelper";
import { AppStateContext, AppHubContext } from "../context/AppContext";
import { useEffect, useContext } from "react";
import { HubMessages } from "../classes/constants/HubMessages";
import * as signalR from "@microsoft/signalr";
import { HttpService } from "../classes/services/HttpService";
import useClipboardApi from "use-clipboard-api";
import { useSignal } from "@preact/signals-react";
import { BsFillLayersFill } from "react-icons/bs";
import { toast } from "react-toastify";
import { ToastNotificationEnum } from "../enums/ToastNotificationEnum";

export default function Header() {
  const appState = useContext(AppStateContext);
  const appHub = useContext(AppHubContext);

  const navigate = useNavigate();
  const [, copy] = useClipboardApi();

  const buttonColor = useSignal<string>("primary");

  const httpService = HttpService.getInstance();
  const localStorageService = LocalStorageService.getInstance();
  const httpUrlHelper = new HttpUrlHelper();

  useEffect(() => {
    appState.username.value = localStorageService.getUsername();
    appState.roomHash.value = httpUrlHelper.getRoomHash(window.location.href);
  }, []);

  useEffect(() => {
    const startAppHubConnection = async () => {
      try {
        appHub.on(HubMessages.OnReceiveConnectionId, (connectionId: string) => {
          appState.connectionId.value = connectionId;
        });

        await appHub.start();

      } catch (error) {
        appState.connectionId.value = null;
      }
    };

    if (appHub.getState() !== signalR.HubConnectionState.Connected) {
      setTimeout(startAppHubConnection, 550);
    }
  }, [appHub]);

  const handleLeaveRoomButtonClick = () => {
    httpService.leaveRoom(appState.roomHash.value);

    appState.isInRoom.value = false;
    navigate(ClientEndpoints.mainMenu);
  };

  const handleCopyToClipboard = () => {
    const clipboardValue = window.location.href.replace("room", "joinRoom");

    copy(clipboardValue);
    buttonColor.value = "success";

    toast.success(
      "Invitation link copied to clipboard", {
      containerId: ToastNotificationEnum.Room
    }
    );

    setTimeout(() => {
      buttonColor.value = "primary";
    }, 500);
  };

  return (
    <div
      className={ !appState.isInRoom.value ? "d-flex justify-content-center align-items-center" : "mx-5" }
      style={{ paddingLeft: '5rem', paddingRight: '5rem' }}
    >
      <nav className="navbar navbar-dark mb-2 mt-2">
        <div className="d-flex align-items-center justify-content-center">
          <div className="navbar-brand ms-3">
            <i><b>Shared Watch</b></i> <BsFillCameraReelsFill />
          </div>
          {!appState.isInRoom.value && (
            <div className="ms-3 me-3">
              <InputField
                classNames={`form-control form-control rounded-3  ${appState.username.value.length < 3 ? "is-invalid" : "is-valid"}`}
                placeholder={"Username (min. 3 chars)"}
                value={appState.username.value}
                trim={true}
                maxCharacters={25}
                isEnabled={true}
                onChange={(value: string) => {
                  appState.username.value = value;
                  localStorageService.setUsername(value);
                }}
              />
            </div>
          )}
          {appState.isInRoom.value && (
            <div className="text-white ms-3 me-3">
              {appState.isAdmin.value ? <BsShieldFillCheck /> : <BsFillPersonFill />}
              <span className="text-break text-orange ms-2">
                <b>{appState.username.value}</b>
              </span>
            </div>
          )}
        </div>
        <div className="d-flex justify-content-end">
          {appState.isInRoom.value && (
            <div className="justify-content-end me-3 mt-header">
              <Button
                text={<><BsFillLayersFill /> Copy invitation link</>}
                classNames={`btn btn-${buttonColor.value} btn-sm me-4 ms-3`}
                onClick={handleCopyToClipboard}
              />
              <Button
                text={<><BsDoorOpenFill /> Leave room</>}
                classNames={"btn btn-danger btn-sm"}
                onClick={handleLeaveRoomButtonClick}
              />
            </div>
          )}
        </div>
      </nav>
    </div>
  );
}
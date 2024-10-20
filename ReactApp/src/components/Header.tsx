import {
  BsDoorOpenFill,
  BsFillCameraReelsFill,
  BsFillPersonFill,
  BsShieldFillCheck,
} from "react-icons/bs";
import { AppStateContext } from "../context/AppContext";
import { useEffect, useContext } from "react";
import useClipboardApi from "use-clipboard-api";
import { useSignal } from "@preact/signals-react";
import { BsFillLayersFill } from "react-icons/bs";
import { toast } from "react-toastify";
import { ToastNotificationEnum } from "../enums/ToastNotificationEnum";
import { useNavigate } from "react-router-dom";
import { HttpService } from "../classes/services/HttpService";
import { SessionStorageService } from "../classes/services/SessionStorageService";
import { HttpUrlHelper } from "../classes/helpers/HttpUrlHelper";
import { ClientEndpoints } from "../classes/constants/ClientEndpoints";
import Button from "./shared/Button";

export default function Header() {
  const appState = useContext(AppStateContext);

  const navigate = useNavigate();
  const [, copy] = useClipboardApi();

  const buttonColor = useSignal<string>("primary");

  const httpService = HttpService.getInstance();
  const localStorageManager = SessionStorageService.getInstance();
  const httpUrlHelper = new HttpUrlHelper();

  useEffect(() => {
    appState.username.value = localStorageManager.getUsername();
    appState.roomHash.value = httpUrlHelper.getRoomHash(window.location.href);
  }, []);

  const handleLeaveRoomButtonClick = () => {
    httpService.leaveRoom(appState.roomHash.value);

    appState.isInRoom.value = false;
    navigate(ClientEndpoints.mainMenu);
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
      <nav className="navbar navbar-dark mb-2 mt-2">
        <div className="d-flex align-items-center">
          <div className="navbar-brand ms-3">
            <i>
              <b>Shared Watch</b>{" "}
            </i>
            <BsFillCameraReelsFill />
          </div>
          <div className="text-white ms-3 me-3">
            {appState.isAdmin.value ? <BsShieldFillCheck /> : <BsFillPersonFill />}
            <span className="text-break text-orange ms-2">
              <b>{appState.username.value}</b>
            </span>
          </div>
        </div>
        <div className="d-flex justify-content-end">
          <div className="justify-content-end me-3 mt-header">
            <Button
              text={
                <>
                  <BsFillLayersFill /> Invitiation Link
                </>
              }
              classNames={`btn btn-${buttonColor.value} btn-sm me-4 ms-3`}
              onClick={handleCopyToClipboard}
            />
            <Button
              text={
                <>
                  <BsDoorOpenFill /> Leave Room
                </>
              }
              classNames={"btn btn-danger btn-sm"}
              onClick={handleLeaveRoomButtonClick}
            />
          </div>
        </div>
      </nav>
    </div>
  );
}

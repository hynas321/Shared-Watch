import {
  BsDoorOpenFill,
  BsFillCameraReelsFill,
  BsFillPersonFill,
  BsShieldFillCheck,
  BsFillLayersFill,
} from "react-icons/bs";
import { InputField } from "./shared/InputField";
import Button from "./shared/Button";
import { useNavigate } from "react-router-dom";
import { ClientEndpoints } from "../classes/constants/ClientEndpoints";
import { SessionStorageService } from "../classes/services/SessionStorageService";
import { HttpUrlHelper } from "../classes/helpers/HttpUrlHelper";
import { AppStateContext } from "../context/AppContext";
import { useEffect, useContext } from "react";
import { HttpService } from "../classes/services/HttpService";
import useClipboardApi from "use-clipboard-api";
import { useSignal } from "@preact/signals-react";
import { toast } from "react-toastify";
import { ToastNotificationEnum } from "../enums/ToastNotificationEnum";

export default function Header() {
  const appState = useContext(AppStateContext);
  const navigate = useNavigate();
  const [, copy] = useClipboardApi();
  const buttonColor = useSignal<string>("primary");

  const httpService = HttpService.getInstance();
  const sessionStorageService = SessionStorageService.getInstance();
  const httpUrlHelper = new HttpUrlHelper();

  useEffect(() => {
    appState.username.value = sessionStorageService.getUsername();
    appState.roomHash.value = httpUrlHelper.getRoomHash(window.location.href);
  }, []);

  const handleLeaveRoom = () => {
    httpService.leaveRoom(appState.roomHash.value);
    appState.isInRoom.value = false;
    navigate(ClientEndpoints.mainMenu);
  };

  const handleCopyToClipboard = () => {
    const clipboardValue = window.location.href.replace("room", "joinRoom");
    copy(clipboardValue);
    buttonColor.value = "success";

    toast.success("Invitation link copied to clipboard", {
      containerId: ToastNotificationEnum.Room,
    });

    setTimeout(() => {
      buttonColor.value = "primary";
    }, 500);
  };

  return (
    <div
      className={
        appState.isInRoom.value ? "mx-5" : "d-flex justify-content-center align-items-center"
      }
      style={{ paddingLeft: "5rem", paddingRight: "5rem" }}
    >
      <nav className="navbar navbar-dark mb-2 mt-2">
        <div className="d-flex align-items-center justify-content-center">
          {renderBrandLogo()}
          {!appState.isInRoom.value && renderUsernameInput()}
          {appState.isInRoom.value && renderUserInfo()}
        </div>
        <div className="d-flex justify-content-end">
          {appState.isInRoom.value && renderRoomButtons()}
        </div>
      </nav>
    </div>
  );

  function renderBrandLogo() {
    return (
      <div className="navbar-brand ms-3">
        <i>
          <b>Shared Watch</b>
        </i>{" "}
        <BsFillCameraReelsFill />
      </div>
    );
  }

  function renderUsernameInput() {
    return (
      <div className="ms-3 me-3">
        <InputField
          classNames={`form-control form-control rounded-3 ${
            appState.username.value.length < 3 ? "is-invalid" : "is-valid"
          }`}
          placeholder="Username (min. 3 chars)"
          value={appState.username.value}
          trim={true}
          maxCharacters={25}
          isEnabled={true}
          onChange={(value: string) => {
            appState.username.value = value;
            sessionStorageService.setUsername(value);
          }}
        />
      </div>
    );
  }

  function renderUserInfo() {
    return (
      <div className="text-white ms-3 me-3">
        {appState.isAdmin.value ? <BsShieldFillCheck /> : <BsFillPersonFill />}
        <span className="text-break text-orange ms-2">
          <b>{appState.username.value}</b>
        </span>
      </div>
    );
  }

  function renderRoomButtons() {
    return (
      <div className="justify-content-end me-3 mt-header">
        <Button
          text={
            <>
              <BsFillLayersFill /> Copy invitation link
            </>
          }
          classNames={`btn btn-${buttonColor.value} btn-sm me-4 ms-3`}
          onClick={handleCopyToClipboard}
        />
        <Button
          text={
            <>
              <BsDoorOpenFill /> Leave room
            </>
          }
          classNames="btn btn-danger btn-sm"
          onClick={handleLeaveRoom}
        />
      </div>
    );
  }
}

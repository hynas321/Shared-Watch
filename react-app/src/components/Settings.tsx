import { useEffect, useState } from "react";
import Switch from "./Switch";
import { InputForm } from "./InputForm";
import Button from "./Button";
import { BsSaveFill } from "react-icons/bs";
import { AppStateContext, roomHub } from "../context/RoomHubContext";
import { useContext } from "react";
import { HubEvents } from "../classes/HubEvents";
import { LocalStorageManager } from "../classes/LocalStorageManager";

export default function Settings() {
  const appState = useContext(AppStateContext);
  const [userPermissions, setUserPermissions] = useState(appState.userPermissions.value);
  const [inputFormPassword, setInputFormPassword] = useState<string>("");

  const localStorageManager = new LocalStorageManager();

  useEffect(() => {
    setUserPermissions(appState.userPermissions.value);
  }, [, appState.userPermissions.value]);

  const handleSetRoomPrivateButtonClick = () => {
    if (inputFormPassword === appState.roomPassword.value) {
      return;
    }

    if (inputFormPassword.length > 0) {
      roomHub.invoke(
        HubEvents.SetRoomPassword,
        appState.roomHash.value,
        localStorageManager.getAuthorizationToken(),
        inputFormPassword
      );
  
      setInputFormPassword("");
    }
    else {
      roomHub.invoke(
        HubEvents.SetRoomPassword,
        appState.roomHash.value,
        localStorageManager.getAuthorizationToken(),
        ""
      );

      setInputFormPassword("");
    }
  }

  const handleRemovePasswordButtonClick = () => {
    roomHub.invoke(
      HubEvents.SetRoomPassword,
      appState.roomHash.value,
      localStorageManager.getAuthorizationToken(),
      ""
    );

    setInputFormPassword("");
  }

  const handleSetRoomPrivateEnterClick = (key: string) => {
    if (key === "Enter") {
      handleSetRoomPrivateButtonClick();
    }
  }

  const invokeChange = () => {
    roomHub.invoke(HubEvents.SetUserPermissions, appState.roomHash.value, localStorageManager.getAuthorizationToken(), appState.userPermissions.value);
  };

  const setCanAddChatMessage = (checked: boolean) => {
    appState.userPermissions.value!.canAddChatMessage = checked;
    invokeChange();
  };

  const setCanAddVideo = (checked: boolean) => {
    appState.userPermissions.value!.canAddVideo = checked;
    invokeChange();
  };

  const setCanRemoveVideo = (checked: boolean) => {
    appState.userPermissions.value!.canRemoveVideo = checked;
    invokeChange();
  };

  const setCanStartOrPauseVideo = (checked: boolean) => {
    appState.userPermissions.value!.canStartOrPauseVideo = checked;
    invokeChange();
  };

  const setCanSkipVideo = (checked: boolean) => {
    appState.userPermissions.value!.canSkipVideo = checked;
    invokeChange();
  };

  return (
    <>
          {
            appState.isAdmin.value &&
            <div className="d-block mb-3">
              <h6 className="text-orange text-center mb-3">Room settings</h6>
              <div className="d-flex">
                <InputForm
                  classNames="form-control rounded-0"
                  placeholder={"Enter password (private room)"}
                  value={inputFormPassword}
                  trim={false}
                  isEnabled={true}
                  onChange={(value: string) => setInputFormPassword(value)}
                  onKeyDown={handleSetRoomPrivateEnterClick}
                />
                <Button
                  text={<><BsSaveFill /></>}
                  classNames="btn btn-primary rounded-0"
                  onClick={handleSetRoomPrivateButtonClick}
                />
              </div>
              { 
                (appState.roomPassword.value.length > 0) && 
                <div className="mt-2">
                  <span className="text-white room-password">Current password: {appState.roomPassword.value} </span>
                  <Button
                    text={"Remove password"}
                    classNames="text-orange button-text"
                    onClick={handleRemovePasswordButtonClick}
                  />
                </div>
              }
            </div>
          }
      <div className="d-block">
        <h6 className="text-orange text-center">User permissions</h6>
        <div className="mt-3">
          <Switch 
            label={"Send chat messages"}
            isChecked={userPermissions?.canAddChatMessage as boolean}
            isEnabled={appState.isAdmin.value}
            onCheckChange={(checked: boolean) => setCanAddChatMessage(checked)} 
          />
        </div>
        <div className="mt-3"> 
          <Switch 
            label={"Add videos to the playlist"}
            isChecked={appState.userPermissions.value?.canAddVideo as boolean}
            isEnabled={appState.isAdmin.value}
            onCheckChange={(checked: boolean) => setCanAddVideo(checked)} 
          />
          <Switch 
            label={"Remove videos from the playlist"}
            isChecked={appState.userPermissions.value?.canRemoveVideo as boolean}
            isEnabled={appState.isAdmin.value}
            onCheckChange={(checked: boolean) => setCanRemoveVideo(checked)} 
          />
        </div>
        <div className="mt-3">
          <Switch 
            label={"Start/Pause videos"}
            isChecked={appState.userPermissions.value?.canStartOrPauseVideo as boolean}
            isEnabled={appState.isAdmin.value}
            onCheckChange={(checked: boolean) => setCanStartOrPauseVideo(checked)} 
          />
          <Switch 
            label={"Skip videos"}
            isChecked={appState.userPermissions.value?.canSkipVideo as boolean}
            isEnabled={appState.isAdmin.value}
            onCheckChange={(checked: boolean) => setCanSkipVideo(checked)} 
          />
        </div>
      </div>
    </>
  )
}

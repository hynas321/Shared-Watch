import { useState } from "react";
import Switch from "./Switch";
import { InputForm } from "./InputForm";
import Button from "./Button";
import { BsSaveFill } from "react-icons/bs";
import FormRange from "./FormRange";
import { AppStateContext } from "../context/RoomHubContext";
import { useContext } from "react";

export default function Settings() {
  const appState = useContext(AppStateContext);
  const [inputFormPassword, setInputFormPassword] = useState<string>("");

  const handleSetRoomPrivateButtonClick = () => {
    if (inputFormPassword === appState.roomPassword.value) {
      return;
    }

    if (inputFormPassword.length > 0) {
      appState.roomPassword.value = inputFormPassword;
      setInputFormPassword("");
    }
    else {
      appState.roomPassword.value = "";
    }
  }

  const handleRemovePasswordButtonClick = () => {
    appState.roomPassword.value = "";
    setInputFormPassword("");
  }

  const handleSetRoomPrivateEnterClick = (key: string) => {
    if (key === "Enter") {
      handleSetRoomPrivateButtonClick();
    }
  }

  const setMaxUsers = (value: number) => {
    appState.maxUsers.value = value;
  }

  const setCanAddChatMessage = (checked: boolean) => {
    appState.userPermissions.value!.canAddChatMessage = checked;
  };

  const setCanAddVideo = (checked: boolean) => {
    appState.userPermissions.value!.canAddVideo = checked;
  };

  const setCanRemoveVideo = (checked: boolean) => {
    appState.userPermissions.value!.canRemoveVideo = checked;
  };

  const setCanPlayVideoOutsideOfPlaylist = (checked: boolean) => {
    appState.userPermissions.value!.canPlayVideoOutsideOfPlaylist = checked;
  };

  const setCanStartOrPauseVideo = (checked: boolean) => {
    appState.userPermissions.value!.canStartOrPauseVideo = checked;
  };

  const setCanSkipVideo = (checked: boolean) => {
    appState.userPermissions.value!.canSkipVideo = checked;
  };

  return (
    <>
      <div className="d-block">
          {
            appState.isAdmin.value &&
            <>
              <h6 className="text-info text-center mb-3">Room settings</h6>
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
                    classNames="text-primary button-text"
                    onClick={handleRemovePasswordButtonClick}
                  />
                </div>
              }
            </>
          }
        <div className="text-white text-center">
          {
            appState.isAdmin.value &&
            <FormRange
              label={"Max users"}
              labelClassNames={"mt-3"}
              minValue={2}
              maxValue={10}
              defaultValue={6}
              step={1}
              onChange={(value: number) => setMaxUsers(value)}
            />
          }
        </div>
      </div>
      <div className="d-block mt-3">
        <h6 className="text-info text-center">User permissions</h6>
        <div className="mt-3">
          <Switch 
            label={"Send chat messages"}
            defaultIsChecked={appState.userPermissions.value?.canAddChatMessage as boolean}
            isEnabled={appState.isAdmin.value}
            onCheckChange={(checked: boolean) => setCanAddChatMessage(checked)} 
          />
        </div>
        <div className="mt-3"> 
          <Switch 
            label={"Add videos to the playlist"}
            defaultIsChecked={appState.userPermissions.value?.canAddVideo as boolean}
            isEnabled={appState.isAdmin.value}
            onCheckChange={(checked: boolean) => setCanAddVideo(checked)} 
          />
          <Switch 
            label={"Remove videos from the playlist"}
            defaultIsChecked={appState.userPermissions.value?.canRemoveVideo as boolean}
            isEnabled={appState.isAdmin.value}
            onCheckChange={(checked: boolean) => setCanRemoveVideo(checked)} 
          />
          <Switch
            label={"Play videos outside of the playlist"}
            defaultIsChecked={appState.userPermissions.value?.canPlayVideoOutsideOfPlaylist as boolean}
            isEnabled={appState.isAdmin.value}
            onCheckChange={(checked: boolean) => setCanPlayVideoOutsideOfPlaylist(checked)} 
          />
        </div>
        <div className="mt-3">
          <Switch 
            label={"Start/Pause videos"}
            defaultIsChecked={appState.userPermissions.value?.canStartOrPauseVideo as boolean}
            isEnabled={appState.isAdmin.value}
            onCheckChange={(checked: boolean) => setCanStartOrPauseVideo(checked)} 
          />
          <Switch 
            label={"Skip videos"}
            defaultIsChecked={appState.userPermissions.value?.canSkipVideo as boolean}
            isEnabled={appState.isAdmin.value}
            onCheckChange={(checked: boolean) => setCanSkipVideo(checked)} 
          />
        </div>
      </div>
    </>
  )
}

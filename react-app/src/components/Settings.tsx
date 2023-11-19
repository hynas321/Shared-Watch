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
    if (inputFormPassword === appState.password.value) {
      return;
    }

    if (inputFormPassword.length > 0) {
      appState.password.value = inputFormPassword;
      setInputFormPassword("");
    }
    else {
      appState.password.value = "";
    }
  }

  const handleRemovePasswordButtonClick = () => {
    appState.password.value = "";
    setInputFormPassword("");
  }

  const handleSetRoomPrivateEnterClick = (key: string) => {
    if (key === "Enter") {
      handleSetRoomPrivateButtonClick();
    }
  }

  const setMaxUsers = (value: number) => {
    appState.roomSettings.value!.maxUsers = value;
  }

  const setIsSendingChatMessagesAllowed = (checked: boolean) => {
    appState.roomSettings.value!.isSendingChatMessagesAllowed = checked;
  };

  const setIsAddingVideosAllowed = (checked: boolean) => {
    appState.roomSettings.value!.isAddingVideosAllowed = checked;
  };

  const setIsRemovingVideosAllowed = (checked: boolean) => {
    appState.roomSettings.value!.isRemovingVideosAllowed = checked;
  };

  const setIsPlayingVideosOutsideOfPlaylistAllowed = (checked: boolean) => {
    appState.roomSettings.value!.isPlayingVideosOutsideOfPlaylistAllowed = checked;
  };

  const setIsStartingPausingVideosAllowed = (checked: boolean) => {
    appState.roomSettings.value!.isStartingPausingVideosAllowed = checked;
  };

  const setIsSkippingVideosAllowed = (checked: boolean) => {
    appState.roomSettings.value!.isSkippingVideosAllowed = checked;
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
                (appState.password.value.length > 0) && 
                <div className="mt-2">
                  <span className="text-white room-password">Current password: {appState.password.value} </span>
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
            defaultIsChecked={appState.roomSettings.value?.isSendingChatMessagesAllowed as boolean}
            isEnabled={appState.isAdmin.value}
            onCheckChange={(checked: boolean) => setIsSendingChatMessagesAllowed(checked)} 
          />
        </div>
        <div className="mt-3"> 
          <Switch 
            label={"Add videos to the playlist"}
            defaultIsChecked={appState.roomSettings.value?.isAddingVideosAllowed as boolean}
            isEnabled={appState.isAdmin.value}
            onCheckChange={(checked: boolean) => setIsAddingVideosAllowed(checked)} 
          />
          <Switch 
            label={"Remove videos from the playlist"}
            defaultIsChecked={appState.roomSettings.value?.isRemovingVideosAllowed as boolean}
            isEnabled={appState.isAdmin.value}
            onCheckChange={(checked: boolean) => setIsRemovingVideosAllowed(checked)} 
          />
          <Switch
            label={"Play videos outside of the playlist"}
            defaultIsChecked={appState.roomSettings.value?.isPlayingVideosOutsideOfPlaylistAllowed as boolean}
            isEnabled={appState.isAdmin.value}
            onCheckChange={(checked: boolean) => setIsPlayingVideosOutsideOfPlaylistAllowed(checked)} 
          />
        </div>
        <div className="mt-3">
          <Switch 
            label={"Start/Pause videos"}
            defaultIsChecked={appState.roomSettings.value?.isStartingPausingVideosAllowed as boolean}
            isEnabled={appState.isAdmin.value}
            onCheckChange={(checked: boolean) => setIsStartingPausingVideosAllowed(checked)} 
          />
          <Switch 
            label={"Skip videos"}
            defaultIsChecked={appState.roomSettings.value?.isSkippingVideosAllowed as boolean}
            isEnabled={appState.isAdmin.value}
            onCheckChange={(checked: boolean) => setIsSkippingVideosAllowed(checked)} 
          />
        </div>
      </div>
    </>
  )
}

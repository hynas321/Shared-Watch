import { useEffect, useState } from "react";
import { BsSaveFill, BsTrashFill } from "react-icons/bs";
import { AppHubContext, AppStateContext } from "../context/AppContext";
import { useContext } from "react";
import { HubMessages } from "../classes/constants/HubMessages";
import { InputField } from "./shared/InputField";
import Button from "./shared/Button";
import Switch from "./shared/Switch";
import { RoomTypesEnum } from "../enums/RoomTypesEnum";

export default function Settings() {
  const appState = useContext(AppStateContext);
  const appHub = useContext(AppHubContext);

  const [userPermissions, setUserPermissions] = useState(appState.userPermissions.value);

  useEffect(() => {
    setUserPermissions(appState.userPermissions.value);
  }, [appState.userPermissions.value]);

  const handleSetRoomPrivateButtonClick = () => {
    void appHub.invoke(HubMessages.SetRoomPassword, appState.roomHash.value, appState.roomPassword.value)
      .catch((error: unknown) => console.warn("Unable to update the room password.", error));
  };

  const handleRemovePasswordButtonClick = () => {
    appHub.invoke(HubMessages.SetRoomPassword, appState.roomHash.value, "");
    appState.roomPassword.value = "";
  };

  const handleSetRoomPrivateEnterClick = (key: string) => {
    if (key === "Enter") {
      handleSetRoomPrivateButtonClick();
    }
  };

  const invokeChange = () => {
    appHub.invoke(HubMessages.SetUserPermissions, appState.roomHash.value, appState.userPermissions.value);
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
      {appState.isAdmin.value && (
        <div className="settings-section settings-room mb-3">
          <h6 className="settings-section-title text-white mb-3">Room settings</h6>
          <label className="form-label text-white mb-1">Room password</label>
          <div className="settings-password-row d-flex flex-nowrap gap-0">
            <InputField
              classNames="form-control rounded-0"
              placeholder={appState.roomPassword.value ? "Room password" : "No password set"}
              value={appState.roomPassword.value}
              trim={false}
              isEnabled={true}
              maxCharacters={35}
              onChange={(value: string) => (appState.roomPassword.value = value)}
              onKeyDown={handleSetRoomPrivateEnterClick}
              type="text"
            />
            <Button
              text={<><BsSaveFill /></>}
              classNames="btn btn-primary rounded-0"
              onClick={handleSetRoomPrivateButtonClick}
            />
            {appState.roomType.value === RoomTypesEnum.private && (
              <Button
                text={<BsTrashFill />}
                classNames="btn btn-danger text-white text-nowrap ms-2"
                onClick={handleRemovePasswordButtonClick}
              />
            )}
          </div>
        </div>
      )}
      <div className="settings-section settings-permissions">
        <h6 className="settings-section-title text-white">User permissions</h6>
        <div className="settings-permission-group mt-3">
          <Switch
            label={"Send chat messages"}
            isChecked={userPermissions?.canAddChatMessage as boolean}
            isEnabled={appState.isAdmin.value}
            onCheckChange={(checked: boolean) => setCanAddChatMessage(checked)}
          />
        </div>
        <div className="settings-permission-group mt-3">
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
        <div className="settings-permission-group mt-3">
          <Switch
            label={"Start/Pause videos"}
            isChecked={appState.userPermissions.value?.canStartOrPauseVideo as boolean}
            isEnabled={appState.isAdmin.value}
            onCheckChange={(checked: boolean) => setCanStartOrPauseVideo(checked)}
          />
          <Switch
            label={"Change video time"}
            isChecked={appState.userPermissions.value?.canSkipVideo as boolean}
            isEnabled={appState.isAdmin.value}
            onCheckChange={(checked: boolean) => setCanSkipVideo(checked)}
          />
        </div>
      </div>
    </>
  );
}

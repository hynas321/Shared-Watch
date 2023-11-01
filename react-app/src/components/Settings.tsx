import { useState } from "react";
import Switch from "./Switch";
import { InputForm } from "./InputForm";
import Button from "./Button";
import { BsSaveFill } from "react-icons/bs";
import FormRange from "./FormRange";

export default function Settings() {
  const [roomPassword, setRoomPassword] = useState<string>("");
  const [inputFormPassword, setInputFormPassword] = useState<string>("");
  const [, setMaxUsers] = useState<number>(6);
  const [isSendingChatMessagesAllowed, setIsSendingChatMessagesAllowed] = useState<boolean>(true);
  const [isAddingVideosAllowed, setIsAddingVideosAllowed] = useState<boolean>(true);
  const [isRemovingVideosAllowed, setIsRemovingVideosAllowed] = useState<boolean>(true);
  const [isPlayingVideosOutsideOfPlaylistAllowed, setIsPlayingVideosOutsideOfPlaylistAllowed] = useState<boolean>(true);
  const [isStartingPausingVideosAllowed, setIsStartingPausingVideosAllowed] = useState<boolean>(false);
  const [isSkippingVideosAllowed, setIsSkippingVideosAllowed] = useState<boolean>(false);

  const handleSetRoomPrivateButtonClick = () => {
    if (inputFormPassword === roomPassword) {
      return;
    }

    if (inputFormPassword.length > 0) {
      setRoomPassword(inputFormPassword);
      setInputFormPassword("");
    }
    else {
      setRoomPassword("");
    }
  }

  const handleRemovePasswordButtonClick = () => {
    setRoomPassword("");
    setInputFormPassword("");
  }

  const handleSetRoomPrivateEnterClick = (key: string) => {
    if (key === "Enter") {
      handleSetRoomPrivateButtonClick();
    }
  }

  return (
    <>
      <div className="d-block">
        <h6 className="text-info text-center mb-3">Room settings</h6>
        <div className="d-flex">
          <InputForm
            classNames="form-control rounded-0"
            placeholder={"Enter password (private room)"}
            value={inputFormPassword}
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
          (roomPassword.length > 0) && 
          <div className="mt-2">
            <span className="text-white room-password">Current password: {roomPassword} </span>
            <Button
              text={"Remove password"}
              classNames="text-primary button-text"
              onClick={handleRemovePasswordButtonClick}
            />
          </div>
        }
        <div className="text-white text-center">
          <FormRange
            label={"Max users"}
            labelClassNames={"mt-3"}
            minValue={2}
            maxValue={10}
            defaultValue={6}
            step={1}
            onChange={(value: number) => setMaxUsers(value)}
          />
        </div>
      </div>
      <div className="d-block mt-3">
        <h6 className="text-info text-center">User permissions</h6>
        <div className="mt-3">
          <Switch 
            label={"Send chat messages"}
            defaultIsChecked={isSendingChatMessagesAllowed}
            onCheckChange={(checked: boolean) => setIsSendingChatMessagesAllowed(checked)} 
          />
        </div>
        <div className="mt-3"> 
          <Switch 
            label={"Add videos to the playlist"}
            defaultIsChecked={isAddingVideosAllowed}
            onCheckChange={(checked: boolean) => setIsAddingVideosAllowed(checked)} 
          />
          <Switch 
            label={"Remove videos from the playlist"}
            defaultIsChecked={isRemovingVideosAllowed}
            onCheckChange={(checked: boolean) => setIsRemovingVideosAllowed(checked)} 
          />
          <Switch
            label={"Play videos outside of the playlist"}
            defaultIsChecked={isPlayingVideosOutsideOfPlaylistAllowed}
            onCheckChange={(checked: boolean) => setIsPlayingVideosOutsideOfPlaylistAllowed(checked)} 
          />
        </div>
        <div className="mt-3">
          <Switch 
            label={"Start/Pause videos"}
            defaultIsChecked={isStartingPausingVideosAllowed}
            onCheckChange={(checked: boolean) => setIsStartingPausingVideosAllowed(checked)} 
          />
          <Switch 
            label={"Skip videos"}
            defaultIsChecked={isSkippingVideosAllowed}
            onCheckChange={(checked: boolean) => setIsSkippingVideosAllowed(checked)} 
          />
        </div>
      </div>
    </>
  )
}

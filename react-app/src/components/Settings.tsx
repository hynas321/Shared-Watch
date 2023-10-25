import { useState } from "react";
import Switch from "./Switch";
import { TextInput } from "./TextInput";
import Button from "./Button";
import { BsSaveFill } from "react-icons/bs";

export default function Settings() {
  const [roomPassword, setRoomPassword] = useState<string>("");
  const [isRoomPrivate, setIsRoomPrivate] = useState<boolean>(false);
  const [isAddingVideosAllowed, setIsAddingVideosAllowed] = useState<boolean>(true);
  const [isRemovingVideosAllowed, setIsRemovingVideosAllowed] = useState<boolean>(false);
  const [isStartingPausingVideosAllowed, setIsStartingPausingVideosAllowed] = useState<boolean>(false);
  const [isSkippingVideosAllowed, setIsSkippingVideosAllowed] = useState<boolean>(false);

  const handleRoomPasswordChange = (value: string) => {
    setIsRoomPrivate(value.length !== 0);
    setRoomPassword(value);
  }

  const handleAddingVideosSettingChange = (checked: boolean) => {
    setIsAddingVideosAllowed(checked);
  }

  const handleRemovingVideosSettingChange = (checked: boolean) => {
    setIsRemovingVideosAllowed(checked);
  }

  const handleStartingPausingVideosSettingChange = (checked: boolean) => {
    setIsStartingPausingVideosAllowed(checked);
  }

  const handleSkippingVideosSettingChange = (checked: boolean) => {
    setIsSkippingVideosAllowed(checked);
  }

  return (
    <>
      <div className="d-block">
        <h6 className="text-info text-center">Room settings</h6>
        <div className="d-flex">
          <TextInput
            classNames="form-control rounded-0"
            placeholder={"Enter password (private room)"}
            value={roomPassword}
            onChange={handleRoomPasswordChange}
            onKeyDown={() => {}}
          />
          <Button
            text={<><BsSaveFill /></>}
            classNames="btn btn-primary rounded-0"
            onClick={() => {}}
          />
        </div>
        { null && <label className="text-white">Max users</label>}
      </div>
      <div className="d-block mt-3">
        <h6 className="text-info text-center">User permissions</h6>
        <Switch 
          label={"Add videos to the playlist"}
          defaultIsChecked={isAddingVideosAllowed}
          onCheckChange={handleAddingVideosSettingChange} 
        />
        <Switch 
          label={"Remove videos from the playlist"}
          defaultIsChecked={isRemovingVideosAllowed}
          onCheckChange={handleRemovingVideosSettingChange} 
        />
        <Switch 
          label={"Start/Pause videos"}
          defaultIsChecked={isStartingPausingVideosAllowed}
          onCheckChange={handleStartingPausingVideosSettingChange} 
        />
        <Switch 
          label={"Skip videos"}
          defaultIsChecked={isSkippingVideosAllowed}
          onCheckChange={handleSkippingVideosSettingChange} 
        />
      </div>
    </>
  )
}

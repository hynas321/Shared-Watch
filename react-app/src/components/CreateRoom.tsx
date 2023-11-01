import { useState } from "react";
import { InputForm } from "./InputForm";

export default function CreateRoom() {
  const [roomPassword, setRoomPassword] = useState<string>("");
  const [roomName, setRoomName] = useState<string>("");

  return (
    <>
      <div className="d-block">
        <h6 className="text-dark text-center mb-3"><b>Room name</b></h6>
          <InputForm
            classNames="form-control rounded-0"
            placeholder={"Enter room name"}
            value={roomName}
            onChange={(value: string) => setRoomName(value)}
          />
      </div>
      <div className="d-block mt-3">
      <h6 className="text-dark text-center mb-3"><b>Room password (optional)</b></h6>
        <InputForm
          classNames="form-control rounded-0"
          placeholder={"Enter password (private room)"}
          value={roomPassword}
          onChange={(value: string) => setRoomPassword(value)}
        />
      </div>
    </>
  )
}

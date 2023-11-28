import { useEffect, useState } from "react";
import Button from "./Button"
import { InputForm } from "./InputForm";
import { HttpManager } from "../classes/HttpManager";
import { useNavigate } from "react-router-dom";
import { ClientEndpoints } from "../classes/ClientEndpoints";
import { HttpStatusCodes } from "../classes/HttpStatusCodes";
import { toast } from "react-toastify";
import { RoomCreateOutput } from "../types/HttpTypes/Output/RoomCreateOutput";
import { RoomState } from "../types/RoomState";
import { appState } from "../context/RoomHubContext";
import { useSignal } from "@preact/signals-react";
import { RoomHelper } from "../classes/RoomHelper";

export interface CreateRoomModalProps {
  title: string;
  acceptText: string;
  declineText: string;
}

export default function CreateRoomModal({title, acceptText, declineText}: CreateRoomModalProps) {
  const [isAcceptButtonEnabled, setIsAcceptButtonEnabled] = useState<boolean>(false);
  const roomName = useSignal<string>("");
  const roomPassword = useSignal<string>("");
  const navigate = useNavigate();

  const httpManager = new HttpManager();
  const roomHelper = new RoomHelper();
  
  useEffect(() => {
    return () => {
      clearInputFields();
    }
  }, []);

  useEffect(() => {
    if (roomName.value.length >= 3) {
      setIsAcceptButtonEnabled(true);
    }
    else {
      setIsAcceptButtonEnabled(false);
    }

  }, [roomName.value]);

  const clearInputFields = () => {
    roomName.value = "";
    roomPassword.value = "";
  }

  const handleCreateRoomButtonClick = async () => {
    const [responseStatusCode, roomInformation]: [number, RoomCreateOutput | undefined] =
      await httpManager.createRoom(
        roomName.value,
        roomPassword.value,
        appState.username.value
      );
    
    if (responseStatusCode !== HttpStatusCodes.CREATED) {
      console.log(responseStatusCode)
      switch(responseStatusCode) {
        case HttpStatusCodes.CONFLICT:
          toast.error("The room with this name already exists");
          break;
        default:
          toast.error("Could not create the room");
          break;
      }

      return;
    }

    const roomState: RoomState = {
      roomHash: roomInformation?.roomHash as string,
      roomName: roomName.value as string,
      password: roomPassword.value as string
    };
    
    const canJoin = await roomHelper.joinRoom(roomState);

    console.log(canJoin);

    if (canJoin) {
      navigate(`${ClientEndpoints.room}/${roomState.roomHash}`, { replace: true });
    }
  }

  return (
    <>
      <div>
        <span className="rounded-1" data-bs-toggle="modal" data-bs-target="#exampleModal">
          <Button
            text={title}
            classNames={`btn btn-success ms-3 ${appState.username.value.length < 3 && "disabled"}`}
            onClick={() => {}}
          />
        </span>
      </div>
      <div className="modal fade" id="exampleModal" tabIndex={-1} role="dialog">
        <div className="modal-dialog" role="document">
          <div className="modal-content">
            <div className="modal-header bg-light">
              <h5 className="modal-title">{title}</h5>
            </div>
            <div className="modal-body bg-light">
              <div className="d-block">
                <h6 className="text-dark text-center mb-3"><b>Room name</b></h6>
                  <InputForm
                    classNames="form-control rounded-0"
                    placeholder="Enter room name (min 3 characters)"
                    value={roomName.value}
                    trim={false}
                    isEnabled={true}
                    onChange={(value: string) => {roomName.value = value}}
                  />
              </div>
              <div className="d-block mt-3">
              <h6 className="text-dark text-center mb-3"><b>Room password (optional)</b></h6>
                <InputForm
                  classNames="form-control rounded-0"
                  placeholder={"Enter password (private room)"}
                  value={roomPassword.value}
                  trim={true}
                  isEnabled={true}
                  onChange={(value: string) => {roomPassword.value = value}}
                />
              </div>
            </div>
            <div className="modal-footer bg-light">
              <span className="rounded-1" {...(isAcceptButtonEnabled ? {'data-bs-dismiss': 'modal'} : {})}>
                <Button
                  text={acceptText}
                  classNames={`btn btn-primary ${!isAcceptButtonEnabled && "disabled"}`}
                  onClick={handleCreateRoomButtonClick}
                />
              </span>
              <span className="rounded-1" data-bs-dismiss="modal">
                <Button
                  text={declineText}
                  classNames={"btn btn-danger"}
                  onClick={clearInputFields}
                />
              </span>
            </div>
          </div>
        </div>
      </div>
    </>
  )
}
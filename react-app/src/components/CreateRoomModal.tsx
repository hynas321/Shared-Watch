import { useEffect, useState } from "react";
import Button from "./Button"
import { InputField } from "./InputField";
import { HttpManager } from "../classes/HttpManager";
import { useNavigate } from "react-router-dom";
import { ClientEndpoints } from "../classes/ClientEndpoints";
import { HttpStatusCodes } from "../classes/HttpStatusCodes";
import { toast } from "react-toastify";
import { RoomCreateOutput } from "../types/HttpTypes/Output/RoomCreateOutput";
import { RoomState } from "../types/RoomState";
import { appHub, appState } from "../context/AppContext";
import { useSignal } from "@preact/signals-react";
import { RoomHelper } from "../classes/RoomHelper";
import { ToastNotificationEnum } from "../enums/ToastNotificationEnum";
import { ping } from 'ldrs'
import { HubMessages } from "../classes/HubEvents";

export interface CreateRoomModalProps {
  acceptText: string;
  declineText: string;
}

export default function CreateRoomModal({acceptText, declineText}: CreateRoomModalProps) {
  const [isAcceptButtonEnabled, setIsAcceptButtonEnabled] = useState<boolean>(false);
  const roomName = useSignal<string>("");
  const roomPassword = useSignal<string>("");
  const isCreateButtonClicked = useSignal<boolean>(false);
  const navigate = useNavigate();

  const httpManager = new HttpManager();
  const roomHelper = RoomHelper.getInstance();

  useEffect(() => {
    ping.register();

    return () => {
      clearInputFields();
    };
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
  };

  const handleCreateRoomButtonClick = async () => {
    const [responseStatusCode, roomInformation]: [number, RoomCreateOutput | undefined] =
      await httpManager.createRoom(
        roomName.value,
        roomPassword.value,
        appState.username.value
      );

    if (responseStatusCode !== HttpStatusCodes.CREATED) {
      switch (responseStatusCode) {
        case HttpStatusCodes.CONFLICT:
          toast.error(
            "The room with this name already exists", {
            containerId: ToastNotificationEnum.Main
          });
          break;
        case HttpStatusCodes.UNAUTHORIZED:
          toast.error(
            "You are already in a different room", {
            containerId: ToastNotificationEnum.Main
          });
          break;
        default:
          toast.error(
            "Could not create the room", {
            containerId: ToastNotificationEnum.Main
          });
          break;
      }
      return;
    }

    const roomState: RoomState = {
      roomHash: roomInformation?.roomHash as string,
      roomName: roomName.value as string,
      password: roomPassword.value as string
    };

    appHub.off(HubMessages.onListOfRoomsUpdated);

    const canJoin = await roomHelper.joinRoom(roomState);

    if (canJoin) {
      navigate(`${ClientEndpoints.room}/${roomState.roomHash}`, { replace: true });
    }
  };

  return (
    <>
      <div>
        <span className="rounded-1" data-bs-toggle="modal" data-bs-target="#exampleModal">
          <Button
            text={"Create"}
            classNames="btn btn-success ms-3"
            onClick={() => {}}
          />
        </span>
      </div>
      <div className="modal fade" id="exampleModal" tabIndex={-1} role="dialog" style={{ marginTop: '3.75rem', backgroundColor: 'rgba(0,0,0,.0001)' }}>
        <div className="modal-dialog" role="document">
          <div className="modal-content">
            <div className="modal-header bg-light">
              <h5 className="modal-title">{"Create new room"}</h5>
            </div>
            <div className="modal-body bg-light">
              <div className="d-block">
                <h6 className="text-dark text-center mb-3"><b>Room name</b></h6>
                <InputField
                  classNames="form-control rounded-0"
                  placeholder="Enter room name (min 3 characters)"
                  value={roomName.value}
                  trim={false}
                  isEnabled={true}
                  maxCharacters={55}
                  onChange={(value: string) => { roomName.value = value }}
                />
              </div>
              <div className="d-block mt-3">
                <h6 className="text-dark text-center mb-3"><b>Room password (optional)</b></h6>
                <InputField
                  classNames="form-control rounded-0"
                  placeholder={"Enter password (private room)"}
                  value={roomPassword.value}
                  trim={true}
                  isEnabled={true}
                  maxCharacters={35}
                  onChange={(value: string) => { roomPassword.value = value }}
                />
              </div>
            </div>
            <div className="modal-footer bg-light">
              {isCreateButtonClicked.value === true &&
                <span className="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
              }
              <span className="rounded-1" {...(isAcceptButtonEnabled ? { 'data-bs-dismiss': 'modal' } : {})}>
                <Button
                  text={`${isCreateButtonClicked.value === true ? "Creating..." : acceptText}`}
                  classNames={`btn btn-primary ${(!isAcceptButtonEnabled || isCreateButtonClicked.value === true) && "disabled"}`}
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
  );
}
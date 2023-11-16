import { useEffect, useState } from "react";
import Button from "./Button"
import { InputForm } from "./InputForm";
import { HttpManager } from "../classes/HttpManager";
import { useNavigate } from "react-router-dom";
import { ClientEndpoints } from "../classes/ClientEndpoints";
import { useAppSelector } from "../redux/hooks";
import { RoomTypesEnum } from "../enums/RoomTypesEnum";
import { HttpStatusCodes } from "../classes/HttpStatusCodes";
import { toast } from "react-toastify";
import { RoomCreateOutput } from "../types/HttpTypes/Output/RoomCreateOutput";
import { RoomNavigationState } from "../types/RoomNavigationState";

export interface CreateRoomModalProps {
  title: string;
  acceptText: string;
  declineText: string;
}

export default function CreateRoomModal({title, acceptText, declineText}: CreateRoomModalProps) {
  const [roomPassword, setRoomPassword] = useState<string>("");
  const [roomName, setRoomName] = useState<string>("");
  const [isAcceptButtonEnabled, setIsAcceptButtonEnabled] = useState<boolean>(false);

  const navigate = useNavigate();
  const userState = useAppSelector((state) => state.userState);

  const httpManager = new HttpManager();

  useEffect(() => {
    if (roomName.length >= 3) {
      setIsAcceptButtonEnabled(true);
    }
    else {
      setIsAcceptButtonEnabled(false);
    }

  }, [roomName]);

  const handleCreateRoomButtonClick = async () => {
    const [responseStatusCode, responseData]: [number, RoomCreateOutput | undefined] = await httpManager.createRoom(roomName, roomPassword, userState.username);
    
    if (responseStatusCode !== HttpStatusCodes.CREATED) {

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

    const navigationState: RoomNavigationState = {
      roomName: roomName,
      roomType: roomPassword.length === 0 ? RoomTypesEnum.public : RoomTypesEnum.private,
      password: roomPassword
    };

    navigate(`${ClientEndpoints.room}/${responseData?.roomHash}`, { state: { ...navigationState } });
  }

  return (
    <>
      <div>
        <span className="rounded-1" data-bs-toggle="modal" data-bs-target="#exampleModal">
          <Button
            text={title}
            classNames={`btn btn-success ms-3 ${userState.username.length < 3 && "disabled"}`}
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
                    value={roomName}
                    trim={false}
                    isEnabled={true}
                    onChange={(value: string) => setRoomName(value)}
                  />
              </div>
              <div className="d-block mt-3">
              <h6 className="text-dark text-center mb-3"><b>Room password (optional)</b></h6>
                <InputForm
                  classNames="form-control rounded-0"
                  placeholder={"Enter password (private room)"}
                  value={roomPassword}
                  trim={true}
                  isEnabled={true}
                  onChange={(value: string) => setRoomPassword(value)}
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
                  onClick={() => {}}
                />
              </span>
            </div>
          </div>
        </div>
      </div>
    </>
  )
}
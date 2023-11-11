import { useEffect, useState } from "react";
import Button from "./Button"
import { InputForm } from "./InputForm";
import { HttpManager } from "../classes/HttpManager";
import { RoomCreateOutput } from "../types/HttpTypes/Output/RoomCreateOutput";
import { useNavigate } from "react-router-dom";
import { ClientEndpoints } from "../classes/ClientEndpoints";
import { useAppSelector } from "../redux/hooks";

export interface CreateRoomModalProps {
  title: string;
  acceptText: string;
  declineText: string;
}

export default function CreateRoomModal({title, acceptText, declineText}: CreateRoomModalProps) {
  const [roomPassword, setRoomPassword] = useState<string>("");
  const [roomName, setRoomName] = useState<string>("");
  const [isAcceptButtonEnabled, setIsAcceptButtonEnabled] = useState<boolean>(false);
  const userState = useAppSelector((state) => state.userState);
  const navigate = useNavigate();

  const httpManager: HttpManager = new HttpManager();

  useEffect(() => {
    if (roomName.length >= 3) {
      setIsAcceptButtonEnabled(true);
    }
    else {
      setIsAcceptButtonEnabled(false);
    }

  }, [roomName]);

  const handleCreateRoomButtonClick = async () => {
    const createRoomOutput: RoomCreateOutput | null = await httpManager.createRoom(roomName, roomPassword, userState.username);

    if (createRoomOutput == null) {
      return;
    }

    localStorage.setItem("accessToken", createRoomOutput.accessToken);
    navigate(`${ClientEndpoints.room}/${createRoomOutput.roomHash}`);
  }

  return (
    <>
      <button
        className={`btn btn-success ${userState.username.length < 3 && "disabled"}`}
        data-bs-toggle="modal"
        data-bs-target="#exampleModal"
      >
        Create new room
      </button>
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
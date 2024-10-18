import { useEffect, useState } from "react";
import Button from "./shared/Button";
import { InputField } from "./shared/InputField";
import { HttpService } from "../classes/services/HttpService";
import { useNavigate } from "react-router-dom";
import { ClientEndpoints } from "../classes/constants/ClientEndpoints";
import { toast } from "react-toastify";
import { RoomCreateOutput } from "../types/HttpTypes/Output/RoomCreateOutput";
import { RoomState } from "../types/RoomState";
import { appState } from "../context/AppContext";
import { useSignal } from "@preact/signals-react";
import { RoomHelper } from "../classes/helpers/RoomHelper";
import { ToastNotificationEnum } from "../enums/ToastNotificationEnum";
import { ping } from "ldrs";
import { HttpStatusCode } from "axios";

export interface CreateRoomModalProps {
  acceptText: string;
  declineText: string;
  isEnabled: boolean;
}

export default function CreateRoomModal({
  acceptText,
  declineText,
  isEnabled,
}: CreateRoomModalProps) {
  const [isAcceptButtonEnabled, setIsAcceptButtonEnabled] = useState<boolean>(false);
  const roomName = useSignal<string>("");
  const roomPassword = useSignal<string>("");
  const isCreateButtonClicked = useSignal<boolean>(false);
  const navigate = useNavigate();

  const httpService = HttpService.getInstance();
  const roomHelper = RoomHelper.getInstance();

  useEffect(() => {
    ping.register();
    return () => clearInputFields();
  }, []);

  useEffect(() => {
    setIsAcceptButtonEnabled(roomName.value.length >= 3);
  }, [roomName.value]);

  const clearInputFields = () => {
    roomName.value = "";
    roomPassword.value = "";
  };

  const handleCreateRoomButtonClick = async () => {
    isCreateButtonClicked.value = true;
    const [statusCode, roomInfo]: [number, RoomCreateOutput | undefined] =
      await httpService.createRoom(roomName.value, roomPassword.value, appState.username.value);

    if (statusCode !== HttpStatusCode.Created) {
      handleRoomCreationError(statusCode);
      isCreateButtonClicked.value = false;
      return;
    }

    const roomState = createRoomState(roomInfo);

    const canJoin = await roomHelper.joinRoom(roomState);

    if (canJoin) {
      navigate(`${ClientEndpoints.room}/${roomState.roomHash}`, { replace: true });
    }

    isCreateButtonClicked.value = false;
  };

  const handleRoomCreationError = (statusCode: number) => {
    const errorMessages: Record<number, string> = {
      [HttpStatusCode.Conflict]: "The room with this name already exists",
      [HttpStatusCode.Unauthorized]: "You are already in a different room",
    };

    const errorMessage = errorMessages[statusCode] || "Could not create the room";

    toast.error(errorMessage, {
      containerId: ToastNotificationEnum.Main,
    });
  };

  const createRoomState = (roomInfo: RoomCreateOutput | undefined): RoomState => ({
    roomHash: roomInfo?.roomHash || "",
    roomName: roomName.value,
    roomPassword: roomPassword.value,
  });

  return (
    <>
      <div className="rounded-1" data-bs-toggle="modal" data-bs-target="#exampleModal">
        <Button
          text="Create Room"
          classNames={`btn btn-success ${!isEnabled && "disabled"}`}
          onClick={() => {}}
        />
      </div>

      <div
        className="modal fade"
        id="exampleModal"
        tabIndex={-1}
        role="dialog"
        style={{ marginTop: "3.75rem", backgroundColor: "rgba(0,0,0,.0001)" }}
      >
        <div className="modal-dialog" role="document">
          <div className="modal-content">
            <div className="modal-header bg-light">
              <h5 className="modal-title">Create new room</h5>
            </div>
            <div className="modal-body bg-light">
              {renderRoomNameInput()}
              {renderRoomPasswordInput()}
            </div>
            <div className="modal-footer bg-light">
              {isCreateButtonClicked.value && (
                <span
                  className="spinner-border spinner-border-sm"
                  role="status"
                  aria-hidden="true"
                ></span>
              )}
              {renderAcceptButton()}
              {renderDeclineButton()}
            </div>
          </div>
        </div>
      </div>
    </>
  );

  function renderRoomNameInput() {
    return (
      <div className="d-block">
        <h6 className="text-dark text-center mb-3">
          <b>Room name</b>
        </h6>
        <InputField
          classNames="form-control rounded-0"
          placeholder="Enter room name (min 3 characters)"
          value={roomName.value}
          trim={false}
          isEnabled={true}
          maxCharacters={55}
          onChange={(value: string) => (roomName.value = value)}
        />
      </div>
    );
  }

  function renderRoomPasswordInput() {
    return (
      <div className="d-block mt-3">
        <h6 className="text-dark text-center mb-3">
          <b>Room password (optional)</b>
        </h6>
        <InputField
          classNames="form-control rounded-0"
          placeholder="Enter password (private room)"
          value={roomPassword.value}
          trim={true}
          isEnabled={true}
          maxCharacters={35}
          onChange={(value: string) => (roomPassword.value = value)}
        />
      </div>
    );
  }

  function renderAcceptButton() {
    return (
      <span
        className="rounded-1"
        {...(isAcceptButtonEnabled ? { "data-bs-dismiss": "modal" } : {})}
      >
        <Button
          text={`${isCreateButtonClicked.value ? "Creating..." : acceptText}`}
          classNames={`btn btn-primary ${
            (!isAcceptButtonEnabled || isCreateButtonClicked.value) && "disabled"
          }`}
          onClick={handleCreateRoomButtonClick}
        />
      </span>
    );
  }

  function renderDeclineButton() {
    return (
      <span className="rounded-1" data-bs-dismiss="modal">
        <Button text={declineText} classNames="btn btn-danger" onClick={clearInputFields} />
      </span>
    );
  }
}

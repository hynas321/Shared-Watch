import { useEffect, useState } from "react";
import Button from "./Button"
import { InputForm } from "./InputForm";
import { HttpManager } from "../classes/HttpManager";
import { useNavigate } from "react-router-dom";
import { ClientEndpoints } from "../classes/ClientEndpoints";
import { RoomTypesEnum } from "../enums/RoomTypesEnum";
import { HttpStatusCodes } from "../classes/HttpStatusCodes";
import { toast } from "react-toastify";
import { RoomCreateOutput } from "../types/HttpTypes/Output/RoomCreateOutput";
import { RoomNavigationState } from "../types/RoomNavigationState";
import { LocalStorageManager } from "../classes/LocalStorageManager";
import { appState } from "../context/RoomHubContext";
import { ChatMessage } from "../types/ChatMessage";
import { QueuedVideo } from "../types/QueuedVideo";
import { RoomSettings } from "../types/RoomSettings";
import { VideoPlayerSettings } from "../types/VideoPlayerSettings";
import { User } from "../types/User";

export interface CreateRoomModalProps {
  title: string;
  acceptText: string;
  declineText: string;
}

export default function CreateRoomModal({title, acceptText, declineText}: CreateRoomModalProps) {
  const [isAcceptButtonEnabled, setIsAcceptButtonEnabled] = useState<boolean>(false);

  const navigate = useNavigate();

  const httpManager = new HttpManager();
  const localStorageManager = new LocalStorageManager();

  useEffect(() => {
    if (appState.roomName.value.length >= 3) {
      setIsAcceptButtonEnabled(true);
    }
    else {
      setIsAcceptButtonEnabled(false);
    }

  }, [appState.roomName.value]);

  const handleCreateRoomButtonClick = async () => {
    const [responseStatusCode, roomInformation]: [number, RoomCreateOutput | undefined] =
      await httpManager.createRoom(
        appState.roomName.value,
        appState.password.value,
        appState.username.value
      );
    
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
      roomHash: roomInformation?.roomHash as string,
      roomName: appState.roomName.value,
      roomType: appState.password.value.length === 0 ? RoomTypesEnum.public : RoomTypesEnum.private,
      password: appState.password.value
    };
    
    accessRoom(navigationState);
  }

  const accessRoom = async (roomNavigationState: RoomNavigationState) => {
    const [responseStatusCode, roomInformation] = await httpManager.joinRoom(
      roomNavigationState.roomHash,
      roomNavigationState.password,
      appState.username.value
    );
    if (responseStatusCode !== HttpStatusCodes.OK) {

      switch(responseStatusCode) {
        case HttpStatusCodes.UNAUTHORIZED:
          toast.error("Wrong room password");
          break;
        case HttpStatusCodes.FORBIDDEN:
          toast.error("Room full");
          break;
        case HttpStatusCodes.NOT_FOUND:
          toast.error("Room not found");
          break;
        case HttpStatusCodes.CONFLICT:
          toast.error("The user is already in the room");
          break;
        default:
          toast.error("Could not join the room");
      }

      return;
    }

    appState.roomHash.value = roomNavigationState.roomHash;
    appState.roomName.value = roomNavigationState.roomName;
    appState.roomType.value = roomNavigationState.roomType;
    appState.password.value = roomNavigationState.password;

    localStorageManager.setAuthorizationToken(roomInformation?.authorizationToken as string);
    appState.isAdmin.value = roomInformation?.isAdmin as boolean;
  
    appState.chatMessages.value = roomInformation?.chatMessages as ChatMessage[];
    appState.queuedVideos.value =  roomInformation?.queuedVideos as QueuedVideo[];
    appState.roomSettings.value = roomInformation?.roomSettings as RoomSettings;
    appState.users.value = roomInformation?.users as User[];
    appState.videoPlayerSettings.value = roomInformation?.videoPlayerSettings as VideoPlayerSettings;

    navigate(`${ClientEndpoints.room}/${appState.roomHash.value}`);
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
                    value={appState.roomName.value}
                    trim={false}
                    isEnabled={true}
                    onChange={(value: string) => { appState.roomName!.value = value }}
                  />
              </div>
              <div className="d-block mt-3">
              <h6 className="text-dark text-center mb-3"><b>Room password (optional)</b></h6>
                <InputForm
                  classNames="form-control rounded-0"
                  placeholder={"Enter password (private room)"}
                  value={appState.password.value}
                  trim={true}
                  isEnabled={true}
                  onChange={(value: string) => { appState.password!.value = value }}
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
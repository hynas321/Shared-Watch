import { toast } from "react-toastify";
import { appState } from "../context/AppContext";
import { RoomTypesEnum } from "../enums/RoomTypesEnum";
import { ToastNotificationEnum } from "../enums/ToastNotificationEnum";
import { HttpStatusCode } from "axios";
import { RoomJoinOutput } from "../types/HttpTypes/Output/RoomJoinOutput";
import api from "../classes/Http/Api";
import { useSessionStorage } from "./useSessionStorage";
import { RoomState } from "../types/RoomState";

export const useRoomJoin = () => {
  const { setAuthorizationToken } = useSessionStorage();

  const joinRoom = async (roomState: RoomState): Promise<boolean> => {
    const [responseStatusCode, roomInformation] = await api.joinRoom(
      roomState.roomHash,
      roomState.roomPassword,
      appState.username.value
    );

    if (responseStatusCode !== HttpStatusCode.Ok || !roomInformation) {
      switch (responseStatusCode) {
        case HttpStatusCode.Unauthorized:
          toast.error("Wrong room password", {
            containerId: ToastNotificationEnum.Main,
          });
          break;
        case HttpStatusCode.Forbidden:
          toast.error("Room is full", {
            containerId: ToastNotificationEnum.Main,
          });
          break;
        case HttpStatusCode.NotFound:
          toast.error("Room does not exist", {
            containerId: ToastNotificationEnum.Main,
          });
          break;
        case HttpStatusCode.Conflict:
          toast.error("The user with this username is already in the room", {
            containerId: ToastNotificationEnum.Main,
          });
          break;
        default:
          toast.error("Could not join the room", {
            containerId: ToastNotificationEnum.Main,
          });
      }
      return false;
    }

    setAppStateInRoom(roomState, roomInformation);
    return true;
  };

  const setAppStateInRoom = (roomState: RoomState, roomInformation: RoomJoinOutput) => {
    appState.roomHash.value = roomState.roomHash;
    appState.roomName.value = roomState.roomName;
    appState.roomType.value = roomInformation?.roomSettings.roomType as RoomTypesEnum;
    appState.maxUsers.value = roomInformation?.roomSettings.maxUsers as number;
    appState.roomPassword.value = roomState.roomPassword;

    setAuthorizationToken(roomInformation?.authorizationToken as string);
    appState.isAdmin.value = roomInformation?.isAdmin as boolean;

    appState.chatMessages.value = roomInformation?.chatMessages ?? [];
    appState.playlistVideos.value = roomInformation?.playlistVideos ?? [];
    appState.userPermissions.value = roomInformation?.userPermissions ?? {};
    appState.users.value = roomInformation?.users ?? [];
    appState.videoPlayer.value = roomInformation?.videoPlayer ?? null;

    appState.joinedViaView.value = true;
  };

  return {
    joinRoom,
  };
};

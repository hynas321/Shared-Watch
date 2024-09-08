import { toast } from "react-toastify";
import { appState } from "../../context/AppContext";
import { HttpStatusCodes } from "../constants/HttpStatusCodes";
import { RoomTypesEnum } from "../../enums/RoomTypesEnum";
import { ChatMessage } from "../../types/ChatMessage";
import { PlaylistVideo } from "../../types/PlaylistVideo";
import { RoomState } from "../../types/RoomState";
import { HttpService } from "../services/HttpService";
import { LocalStorageService } from "../services/LocalStorageService";
import { User } from "../../types/User";
import { UserPermissions } from "../../types/UserPermissions";
import { VideoPlayer } from "../../types/VideoPlayer";
import { ToastNotificationEnum } from "../../enums/ToastNotificationEnum";

export class RoomHelper {
  private static instance: RoomHelper;

  private httpService: HttpService;
  private localStorageService: LocalStorageService;

  private constructor() {
    this.httpService = HttpService.getInstance();
    this.localStorageService = LocalStorageService.getInstance();
  }

  public static getInstance(): RoomHelper {
    if (!RoomHelper.instance) {
      RoomHelper.instance = new RoomHelper();
    }
    return RoomHelper.instance;
  }

  public joinRoom = async (roomState: RoomState): Promise<boolean> => {
      const [responseStatusCode, roomInformation] = await this.httpService.joinRoom(
        roomState.roomHash,
        roomState.password,
        appState.username.value
      );

      if (responseStatusCode !== HttpStatusCodes.OK) {
        switch(responseStatusCode) {
          case HttpStatusCodes.UNAUTHORIZED:
            toast.error(
              "Wrong room password", {
                containerId: ToastNotificationEnum.Main
              }
            );
            break;
          case HttpStatusCodes.FORBIDDEN:
            toast.error(
              "Room is full", {
                containerId: ToastNotificationEnum.Main
              }
            );
            break;
          case HttpStatusCodes.NOT_FOUND:
            toast.error(
              "Room does not exist", {
                containerId: ToastNotificationEnum.Main
              }
            );
            break;
          case HttpStatusCodes.CONFLICT:
            toast.error(
              "The user with this username is already in the room", {
                containerId: ToastNotificationEnum.Main
              }
            );
            break;
          default:
            toast.error(
              "Could not join the room", {
                containerId: ToastNotificationEnum.Main
              }
            );
        }
  
        return false
      }
  
      appState.roomHash.value = roomState.roomHash;
      appState.roomName.value = roomState.roomName;
      appState.roomType.value = roomInformation?.roomSettings.roomType as RoomTypesEnum;
      appState.maxUsers.value = roomInformation?.roomSettings.maxUsers as number;
      appState.roomPassword.value = roomState.password;
  
      this.localStorageService.setAuthorizationToken(roomInformation?.authorizationToken as string);
      appState.isAdmin.value = roomInformation?.isAdmin as boolean;
  
      appState.chatMessages.value = roomInformation?.chatMessages as ChatMessage[];
      appState.playlistVideos.value =  roomInformation?.playlistVideos as PlaylistVideo[];
      appState.userPermissions.value = roomInformation?.userPermissions as UserPermissions;
      appState.users.value = roomInformation?.users as User[];
      appState.videoPlayer.value = roomInformation?.videoPlayer as VideoPlayer;
  
      appState.joinedViaView.value = true;

      return true;
    }

    checkIfIsYouTubeVideoLink(url: string): boolean {
      const pattern = /(?:https?:\/\/)?(?:www\.)?(?:youtube\.com\/(?:[^\/\n\s]+\/\S+\/|(?:v|e(?:mbed)?)\/|\S*?[?&]v=)|youtu\.be\/)([a-zA-Z0-9_-]{11})/;
      const regex = new RegExp(pattern);
      const match = url.match(regex);
    
      return !!match;
    }
}
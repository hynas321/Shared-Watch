import { toast } from "react-toastify";
import { appState } from "../context/RoomHubContext";
import { HttpStatusCodes } from "./HttpStatusCodes";
import { RoomTypesEnum } from "../enums/RoomTypesEnum";
import { ChatMessage } from "../types/ChatMessage";
import { PlaylistVideo } from "../types/PlaylistVideo";
import { RoomState } from "../types/RoomState";
import { HttpManager } from "./HttpManager";
import { LocalStorageManager } from "./LocalStorageManager";
import { User } from "../types/User";
import { UserPermissions } from "../types/UserPermissions";
import { VideoPlayerState } from "../types/VideoPlayerState";

export class RoomHelper {
    private httpManager = new HttpManager();
    private localStorageManager = new LocalStorageManager();
    
    joinRoom = async (roomState: RoomState): Promise<boolean> => {
        const [responseStatusCode, roomInformation] = await this.httpManager.joinRoom(
          roomState.roomHash,
          roomState.password,
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
    
          return false
        }
    
        appState.roomHash.value = roomState.roomHash;
        appState.roomName.value = roomState.roomName;
        appState.roomType.value = roomInformation?.roomSettings.roomType as RoomTypesEnum;
        appState.maxUsers.value = roomInformation?.roomSettings.maxUsers as number;
        appState.roomPassword.value = roomState.password;
    
        this.localStorageManager.setAuthorizationToken(roomInformation?.authorizationToken as string);
        appState.isAdmin.value = roomInformation?.isAdmin as boolean;
    
        appState.chatMessages.value = roomInformation?.chatMessages as ChatMessage[];
        appState.playlistVideos.value =  roomInformation?.playlistVideos as PlaylistVideo[];
        appState.userPermissions.value = roomInformation?.userPermissions as UserPermissions;
        appState.users.value = roomInformation?.users as User[];
        appState.videoPlayerState.value = roomInformation?.videoPlayerState as VideoPlayerState;
    
        appState.joinedViaView.value = true;

        return true;
      }
    
}
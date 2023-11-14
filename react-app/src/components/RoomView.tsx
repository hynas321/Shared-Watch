import { useDispatch } from "react-redux";
import ControlPanel from "./ControlPanel";
import VideoPlayer from "./VideoPlayer";
import { useEffect, useState } from "react";
import { updatedIsInRoom } from "../redux/slices/userState-slice";
import { HttpManager } from "../classes/HttpManager";
import { useLocation, useNavigate } from "react-router-dom";
import { useAppSelector } from "../redux/hooks";
import { ClientEndpoints } from "../classes/ClientEndpoints";
import Header from "./Header";
import { toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import { HttpStatusCodes } from "../classes/HttpStatusCodes";
import { ChatMessage } from "../types/ChatMessage";
import { QueuedVideo } from "../types/QueuedVideo";
import { RoomSettings } from "../types/RoomSettings";
import { User } from "../types/User";
import { VideoPlayerSettings } from "../types/VideoPlayerSettings";
import { LocalStorageManager } from "../classes/LocalStorageManager";
import { HttpUrlHelper } from "../classes/HttpUrlHelper";
import { NavigationState } from "../types/NavigationState";

export default function RoomView() {
  const [roomHash, setRoomHash] = useState<string>("");
  const [isDataLoaded, setIsDataLoaded] = useState<boolean>();
  const [initialChatMessages, setInitialChatMessages] = useState<ChatMessage[]>([]);
  const [initialQueuedVideos, setInitialQueuedVideos] = useState<QueuedVideo[]>([]);
  const [initialUsers, setInitialUsers] = useState<User[]>([]);
  const [initialRoomSettings, setInitialRoomSettings] = useState<RoomSettings>({} as RoomSettings);
  const [initialVideoPlayerSettings, setInitialVideoPlayerSettings] = useState<VideoPlayerSettings>({} as VideoPlayerSettings);

  const location = useLocation();
  const { ...navigationState }: NavigationState = location.state ?? false;

  const dispatch = useDispatch();
  const navigate = useNavigate();
  const userState = useAppSelector((state) => state.userState);

  const httpManager = new HttpManager();
  const httpUrlHelper = new HttpUrlHelper();
  const localStorageManager = new LocalStorageManager();
  
  useEffect(() => {
    const hash: string = httpUrlHelper.getRoomHash(window.location.href);
    console.log(navigationState);
    console.log(hash);
    if (hash || hash.length === 0) {
      toast.error("Room not found");
    }

    setRoomHash(hash);

    return () => {
      //TODO
    };
  }, []);

  useEffect(() => {
    if (!roomHash) {
      return;
    }

    joinRoom();
    setIsDataLoaded(true);
  }, [roomHash]);

  const joinRoom = async () => {
    const [responseStatusCode, responseData] = await httpManager.joinRoom(roomHash, navigationState.password, userState.username);

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

      navigate(`${ClientEndpoints.mainMenu}`);
      return;
    }

    localStorageManager.setAuthorizationToken(responseData?.authorizationToken as string);
    dispatch(updatedIsInRoom(true));

    setInitialChatMessages(responseData?.chatMessages as ChatMessage[]);
    setInitialQueuedVideos(responseData?.queuedVideos as QueuedVideo[]);
    setInitialUsers(responseData?.users as User[]);
    setInitialRoomSettings(responseData?.roomSettings as RoomSettings);
    setInitialVideoPlayerSettings(responseData?.videoPlayerSettings as VideoPlayerSettings);
  }

  /*const leaveRoom = async() => {
    const leaveRoomOutput = await httpManager.leaveRoom(roomHash);
    navigate(`${ClientEndpoints.mainMenu}`);
  };
*/

  useEffect(() => {

  }, [isDataLoaded]); 

  return (
    <>
      <Header />
      <div className="container">
        <div className="row">
          <div className="col-xl-8 col-lg-12 mt-2">
            <VideoPlayer videoPlayerSettings={{url: "abc", isPlaying: false}}/>
          </div>
          <div className="col-xl-4 col-lg-12 mt-2">
            <ControlPanel
              initialChatMessages={initialChatMessages ?? []}
              initialQueuedVideos={initialQueuedVideos ?? []}
              initialUsers={initialUsers ?? []}
              initialRoomSettings={initialRoomSettings ?? {} as RoomSettings}
            />
          </div>
        </div>
      </div>
    </>
  )
}

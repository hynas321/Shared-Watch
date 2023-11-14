import { useDispatch } from "react-redux";
import ControlPanel from "./ControlPanel";
import VideoPlayer from "./VideoPlayer";
import { useEffect, useState } from "react";
import { updatedIsInRoom } from "../redux/slices/userState-slice";
import { HttpManager } from "../classes/HttpManager";
import { useNavigate } from "react-router-dom";
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

export default function RoomView() {
  const [isDataLoaded, setIsDataLoaded] = useState<boolean>();
  const [initialChatMessages, setInitialChatMessages] = useState<ChatMessage[]>([]);
  const [initialQueuedVideos, setInitialQueuedVideos] = useState<QueuedVideo[]>([]);
  const [initialUsers, setInitialUsers] = useState<User[]>([]);
  const [initialRoomSettings, setInitialRoomSettings] = useState<RoomSettings>({} as RoomSettings);
  const [initialVideoPlayerSettings, setInitialVideoPlayerSettings] = useState<VideoPlayerSettings>({} as VideoPlayerSettings);

  const dispatch = useDispatch();
  const navigate = useNavigate();
  const userState = useAppSelector((state) => state.userState);
  const roomState = useAppSelector((state) => state.roomState);

  const httpManager = new HttpManager();
  const localStorageManager = new LocalStorageManager();
  
  const joinRoom = async () => {
    const [responseStatusCode, responseData] = await httpManager.joinRoom(roomState.roomHash, roomState.password, userState.username);

    if (responseStatusCode !== HttpStatusCodes.OK) {
      
      console.log(localStorageManager.getAuthorizationToken());
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
    console.log(responseData)
    setInitialChatMessages(responseData?.chatMessages as ChatMessage[]);
    setInitialQueuedVideos(responseData?.queuedVideos as QueuedVideo[]);
    setInitialUsers(responseData?.users as User[]);
    setInitialRoomSettings(responseData?.roomSettings as RoomSettings);
    setInitialVideoPlayerSettings(responseData?.videoPlayerSettings as VideoPlayerSettings);
  }

  const leaveRoom = async() => {
    const leaveRoomOutput = await httpManager.leaveRoom(roomState.roomHash);
    navigate(`${ClientEndpoints.mainMenu}`);
  };

  useEffect(() => {
    console.log("test")
    joinRoom();
    setIsDataLoaded(true);
    return () => {
      //TODO
    };
  }, []);

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

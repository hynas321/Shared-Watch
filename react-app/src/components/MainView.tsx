import { useContext, useEffect, useState } from "react";
import { Room } from "../types/Room";
import RoomList from "./RoomList";
import { useNavigate } from "react-router-dom";
import { ClientEndpoints } from "../classes/ClientEndpoints"; 
import { InputForm } from "./InputForm";
import Switch from "./Switch";
import { HttpManager } from "../classes/HttpManager";
import Header from "./Header";
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import { HttpStatusCodes } from "../classes/HttpStatusCodes";
import { RoomNavigationState } from "../types/RoomNavigationState";
import { animated, useSpring } from "@react-spring/web";
import CreateRoomModal from "./CreateRoomModal";
import { LocalStorageManager } from "../classes/LocalStorageManager";
import { AppStateContext } from "../context/RoomHubContext";
import { ChatMessage } from "../types/ChatMessage";
import { QueuedVideo } from "../types/QueuedVideo";
import { RoomSettings } from "../types/RoomSettings";
import { User } from "../types/User";
import { VideoPlayerSettings } from "../types/VideoPlayerSettings";

export default function MainView() {
  const appState = useContext(AppStateContext);
  const navigate = useNavigate();

  const [rooms, setRooms] = useState<Room[]>([]);
  const [displayedRooms, setDisplayedRooms] = useState<Room[]>([]);
  const [displayOnlyAvailableRooms, setDisplayOnlyAvailableRooms] = useState<boolean>(false);
  const [searchText, setSearchText] = useState<string>("");

  const httpManager: HttpManager = new HttpManager();
  const localStorageManager: LocalStorageManager = new LocalStorageManager();

  const fetchRooms = async () => {
    const [responseStatusCode, responseData]: [number, Room[] | undefined] = await httpManager.getAllRooms();

    if (responseStatusCode !== HttpStatusCodes.OK) {
      toast.error("Could not load the rooms");
    }

    setRooms(responseData ?? []);
    setDisplayedRooms(responseData ?? []);
  }
  
  useEffect(() => {
    appState.isInRoom.value = false;
    fetchRooms();

    const username = localStorage.getItem("username");

    if (!username) {
      return;
    }
    
    appState.username.value = username;
  }, []);

  useEffect(() => {
    if (rooms.length === 0) {
      return;
    }
    
    if (displayOnlyAvailableRooms) {
      const filteredRooms = rooms.filter((room) =>
        room.roomName.toLowerCase().includes(searchText.toLowerCase()) &&
        room.occupiedSlots !== room.totalSlots
      );

      setDisplayedRooms(filteredRooms);
    }
    else {
      const filteredRooms = rooms.filter((room) =>
        room.roomName.toLowerCase().includes(searchText.toLowerCase())
      );

      setDisplayedRooms(filteredRooms);
    }

  }, [searchText]);

  useEffect(() => {
    if (rooms.length === 0) {
      return;
    }
    
    if (displayOnlyAvailableRooms && searchText.length > 0) {
      const filteredRooms = rooms.filter((room) =>
        room.roomName.toLowerCase().includes(searchText.toLowerCase()) &&
        room.occupiedSlots !== room.totalSlots
      );

      setDisplayedRooms(filteredRooms);
    }
    else if (displayOnlyAvailableRooms && searchText.length === 0) {
      const filteredRooms = rooms.filter((room) =>
        room.occupiedSlots !== room.totalSlots
      );

      setDisplayedRooms(filteredRooms);
    }
    else if (searchText.length > 0) {
      const filteredRooms = rooms.filter((room) =>
        room.roomName.toLowerCase().includes(searchText.toLowerCase())
      );

      setDisplayedRooms(filteredRooms);
    }
    else {
      setDisplayedRooms(rooms);
    }

  }, [displayOnlyAvailableRooms]);

  const handlePublicRoomListItemClick = (item: Room) => {
    const roomNavigationState: RoomNavigationState = {
      roomHash: item.roomHash,
      roomName: item.roomName,
      roomType: item.roomType,
      password: ""
    };

    joinRoom(roomNavigationState);
  };

  const handlePrivateRoomListItemClick = (item: Room, password: string) => {
    const roomNavigationState: RoomNavigationState = {
      roomHash: item.roomHash,
      roomName: item.roomName,
      roomType: item.roomType,
      password: password
    };

    joinRoom(roomNavigationState);
  }

  const joinRoom = async (roomNavigationState: RoomNavigationState) => {
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

    navigate(`${ClientEndpoints.room}/${roomNavigationState.roomHash}`);
  }

  const springs = useSpring({
    from: { y: 200 },
    to: { y: 0 },
    config: {
      mass: 1,
      tension: 250,
      friction: 15
    }
  })

  return (
  <>
    <Header />
    <ToastContainer
      position="top-right"
      autoClose={3000}
      hideProgressBar={false}
      closeOnClick={true}
      draggable={true}
      pauseOnHover={false}
      theme="dark"
      style={{opacity: 0.9}}
    />
    <div className="container">
      <div className="row justify-content-center">
        <animated.div className="main-menu-panel mt-3 col-xl-6 col-lg-6 col-md-8 col-10 bg-dark bg-opacity-50 py-3 px-5 rounded-4" style={{...springs}}>
          <h3 className="text-white text-center mt-3 mb-3">Rooms</h3>
          <div className="row d-flex justify-content-between align-items-center text-center">
            <div className="col-7">
              <InputForm
                classNames="form-control rounded-3 disabled"
                placeholder="Search room name"
                value={searchText}
                trim={false}
                isEnabled={appState.username.value.length >= 3}
                onChange={(value: string) => setSearchText(value)}
              />
            </div>
            <div className="col-5 text-end">
            <CreateRoomModal
              title="Create room"
              acceptText="Create"
              declineText="Go back"
            />
            </div>
          </div>
          <div className="mt-3 mb-3">
            <Switch
              label="Show only available rooms"
              defaultIsChecked={displayOnlyAvailableRooms as boolean}
              isEnabled={appState.username.value.length >= 3}
              onCheckChange={(value: boolean) => setDisplayOnlyAvailableRooms(value)}
            />
          </div>
          {
            displayedRooms.length === 0 &&
              <h5 className="text-danger text-center mt-5 mb-5"><b><i>No rooms found</i></b></h5>
          }
          {
            displayedRooms.length !== 0 &&
            <div className="main-menu-list mb-3">
              <RoomList
                list={displayedRooms}
                onPublicRoomClick={handlePublicRoomListItemClick}
                onPrivateRoomClick={handlePrivateRoomListItemClick}
              />
            </div>
          }
        </animated.div>
      </div>
    </div>
  </>
  )
}

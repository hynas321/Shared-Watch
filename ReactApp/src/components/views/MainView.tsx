import { useContext, useEffect, useState } from "react";
import { Room } from "../../types/Room";
import RoomList from "../RoomList";
import { useNavigate } from "react-router-dom";
import { ClientEndpoints } from "../../classes/constants/ClientEndpoints"; 
import { InputField } from "../InputField";
import Switch from "../Switch";
import { HttpService } from "../../classes/services/HttpService";
import Header from "../Header";
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import { HttpStatusCodes } from "../../classes/constants/HttpStatusCodes";
import { RoomState } from "../../types/RoomState";
import { animated, useSpring } from "@react-spring/web";
import CreateRoomModal from "../CreateRoomModal";
import { AppStateContext, appHub } from "../../context/AppContext";
import { RoomHelper } from "../../classes/helpers/RoomHelper";
import * as signalR from "@microsoft/signalr";
import { HubMessages } from "../../classes/constants/HubMessages";
import { BsDoorOpenFill, BsExclamationTriangleFill } from "react-icons/bs";
import { BsFillPersonLinesFill } from "react-icons/bs";
import { ToastNotificationEnum } from "../../enums/ToastNotificationEnum";
import { helix, ping } from 'ldrs'

export default function MainView() {
  const appState = useContext(AppStateContext);
  const navigate = useNavigate();

  const [rooms, setRooms] = useState<Room[]>([]);
  const [displayedRooms, setDisplayedRooms] = useState<Room[]>([]);
  const [displayOnlyAvailableRooms, setDisplayOnlyAvailableRooms] = useState<boolean>(false);
  const [searchText, setSearchText] = useState<string>("");
  const [areRoomsFetched, setAreRoomsFetched] = useState<boolean>(false);

  const httpService: HttpService = HttpService.getInstance();;
  const roomHelper = RoomHelper.getInstance();

  useEffect(() => {
    if (appHub.getState() !== signalR.HubConnectionState.Connected) {
      return;
    }

    appHub.on(HubMessages.onListOfRoomsUpdated, (listOfRoomsSerialized: string) => {
      const rooms: Room[] = JSON.parse(listOfRoomsSerialized);

      setRooms(rooms);
    });

    return () => {
      appHub.off(HubMessages.onListOfRoomsUpdated);
    }
  }, [appHub.getState()]);

  const fetchRooms = async () => {
    const [responseStatusCode, responseData]: [number, Room[] | undefined] = await httpService.getAllRooms();

    if (responseStatusCode !== HttpStatusCodes.OK) {
      toast.error("Could not load the rooms", {
          containerId: ToastNotificationEnum.Main
        }
      );
    }

    setRooms(responseData ?? []);
    setDisplayedRooms(responseData ?? []);

    setTimeout(() => {
      setAreRoomsFetched(true);
    }, 500);

    setSearchText(".");
    setSearchText("");
  }
  
  useEffect(() => {
    helix.register();
    ping.register();
    appState.isInRoom.value = false;
    fetchRooms();

    const username = localStorage.getItem("username");

    if (!username) {
      return;
    }
    
    appState.username.value = username;
  }, []);

  useEffect(() => {
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

  }, [searchText, rooms]);

  useEffect(() => {
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

  }, [displayOnlyAvailableRooms, rooms]);

  const handlePublicRoomListItemClick = async (item: Room) => {
    const roomState: RoomState = {
      roomHash: item.roomHash,
      roomName: item.roomName,
      password: ""
    };

    const canJoin = await roomHelper.joinRoom(roomState);

    if (canJoin) {
      navigate(`${ClientEndpoints.room}/${roomState.roomHash}`, { replace: true });
    }
  };

  const handlePrivateRoomListItemClick = async (item: Room, password: string) => {
    const roomState: RoomState = {
      roomHash: item.roomHash,
      roomName: item.roomName,
      password: password
    };

    const canJoin = await roomHelper.joinRoom(roomState);

    if (canJoin) {
      navigate(`${ClientEndpoints.room}/${roomState.roomHash}`, { replace: true });
    }
  }

  const springs = useSpring({
    from: { y: 400 },
    to: { y: 0 },
    config: {
      mass: 1.75,
      tension: 150,
      friction: 20,
    }
  });

  return (
    <>
      <Header />
      <ToastContainer
        containerId={ToastNotificationEnum.Main}
        position="top-right"
        autoClose={3000}
        hideProgressBar={true}
        closeOnClick={true}
        draggable={true}
        pauseOnHover={false}
        theme="dark"
        style={{ top: '0px', opacity: 0.9 }}
      />
      <div className="container">
        <div className="row justify-content-center">
          <animated.div className="main-menu-panel mt-3 col-xl-6 col-lg-7 col-md-9 col-11 bg-dark bg-opacity-50 py-3 px-5 rounded-4" style={{ ...springs }}>
            <h3 className="text-white text-center mt-3 mb-3">Rooms</h3>
            {appState.username.value.length >= 3 && appState.connectionId.value != undefined && (
              <div className="mt-4">
                <div className="row d-flex justify-content-between align-items-center text-center">
                  <div className="col-6">
                    <InputField
                      classNames="form-control rounded-3 disabled"
                      placeholder="Search room name"
                      value={searchText}
                      trim={false}
                      isEnabled={true}
                      maxCharacters={60}
                      onChange={(value: string) => setSearchText(value)}
                    />
                  </div>
                  <div className="col-6 text-end">
                    <CreateRoomModal acceptText="Create" declineText="Go back" />
                  </div>
                </div>
                <div className="mt-3 mb-3">
                  <Switch
                    label="Show only available rooms"
                    isChecked={displayOnlyAvailableRooms as boolean}
                    isEnabled={true}
                    onCheckChange={(value: boolean) => setDisplayOnlyAvailableRooms(value)}
                  />
                </div>
              </div>
            )}
            {appState.connectionId.value === undefined && (
              <h1 className="text-white text-center" style={{ marginTop: '9rem' }}>
                <l-helix size="100" speed="1.25" color="white"></l-helix>
              </h1>
            )}
            {displayedRooms.length === 0 && appState.username.value.length >= 3 && appState.connectionId.value != undefined && areRoomsFetched === true && (
              <>
                <h1 className="text-white text-center" style={{ marginTop: '4rem' }}>
                  <BsDoorOpenFill />
                </h1>
                <h5 className="text-white text-center">No rooms to display</h5>
              </>
            )}
            {appState.username.value.length < 3 && appState.connectionId.value != undefined && (
              <>
                <h1 className="text-white text-center" style={{ marginTop: '9rem' }}>
                  <BsFillPersonLinesFill />
                </h1>
                <h5 className="text-white text-center">Enter your username</h5>
                <h6 className="text-white text-center">To gain access to room functionalities</h6>
              </>
            )}
            {appState.connectionId.value === null && (
              <>
                <h1 className="text-white text-center" style={{ marginTop: '9rem' }}>
                  <BsExclamationTriangleFill />
                </h1>
                <h5 className="text-white text-center">Connection Issue</h5>
                <h6 className="text-white text-center">Check your internet connection or refresh the page</h6>
              </>
            )}
            {appState.connectionId.value != null && areRoomsFetched === false && (
              <h1 className="text-white text-center" style={{ marginTop: '6rem' }}>
                <l-ping size="100" speed="1.25" color="white"></l-ping>
              </h1>
            )}
            {displayedRooms.length !== 0 &&
              appState.username.value.length >= 3 &&
              appState.connectionId.value != null &&
              areRoomsFetched === true && (
                <div className="main-menu-list mb-3">
                  <RoomList
                    list={displayedRooms}
                    onPublicRoomClick={handlePublicRoomListItemClick}
                    onPrivateRoomClick={handlePrivateRoomListItemClick}
                  />
                </div>
              )}
          </animated.div>
        </div>
      </div>
    </>
  );
}

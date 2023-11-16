import { useEffect, useState } from "react";
import { Room } from "../types/Room";
import RoomList from "./RoomList";
import { useNavigate } from "react-router-dom";
import { ClientEndpoints } from "../classes/ClientEndpoints"; 
import { InputForm } from "./InputForm";
import Switch from "./Switch";
import { HttpManager } from "../classes/HttpManager";
import { useDispatch } from "react-redux";
import { updatedIsInRoom, updatedUsername } from "../redux/slices/userState-slice";
import { useAppSelector } from "../redux/hooks";
import Header from "./Header";
import { ToastContainer, toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import { HttpStatusCodes } from "../classes/HttpStatusCodes";
import { RoomNavigationState } from "../types/RoomNavigationState";
import { animated, useSpring } from "@react-spring/web";
import CreateRoomModal from "./CreateRoomModal";

export default function MainView() {
  const [rooms, setRooms] = useState<Room[]>([]);
  const [displayedRooms, setDisplayedRooms] = useState<Room[]>([]);
  const [displayOnlyAvailableRooms, setDisplayOnlyAvailableRooms] = useState<boolean>(false);
  const [searchText, setSearchText] = useState<string>("");
  const userState = useAppSelector((state) => state.userState);
  const dispatch = useDispatch();
  const navigate = useNavigate();

  const httpManager: HttpManager = new HttpManager();

  const fetchRooms = async () => {
    const [responseStatusCode, responseData]: [number, Room[] | undefined] = await httpManager.getAllRooms();

    if (responseStatusCode !== HttpStatusCodes.OK) {
      toast.error("Could not load the rooms");
    }

    setRooms(responseData ?? []);
    setDisplayedRooms(responseData ?? []);
  }
  
  useEffect(() => {
    dispatch(updatedIsInRoom(false));
    fetchRooms();

    const username = localStorage.getItem("username");

    if (!username) {
      return;
    }
  
    dispatch(updatedUsername(username));
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
      roomName: item.roomName,
      roomType: item.roomType,
      password: ""
    };

    navigate(`${ClientEndpoints.room}/${item.roomHash}`, { state: { ...roomNavigationState } });
  };

  const handlePrivateRoomListItemClick = (item: Room, password: string) => {
    const navigationState: RoomNavigationState = {
      roomName: item.roomName,
      roomType: item.roomType,
      password: password
    };

    navigate(`${ClientEndpoints.room}/${item.roomHash}`, { state: { ...navigationState } });
  }

  const springs = useSpring({
    from: { y: 200 },
    to: { y: 0 },
    config: {
      mass: 1,
      tension: 250,
      friction:15
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
                isEnabled={userState.username.length >= 3}
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
              isEnabled={userState.username.length >= 3}
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

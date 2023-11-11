import { useEffect, useState } from "react";
import { Room } from "../types/Room";
import RoomList from "./RoomList";
import { useNavigate } from "react-router-dom";
import { ClientEndpoints } from "../classes/ClientEndpoints";
import CreateRoomModal from "./CreateRoomModal";
import { InputForm } from "./InputForm";
import Switch from "./Switch";
import { HttpManager } from "../classes/HttpManager";
import { useDispatch } from "react-redux";
import { updatedIsInRoom, updatedUsername } from "../redux/slices/userState-slice";
import { useAppSelector } from "../redux/hooks";
import Alert from "./Alert";
import { BsFillCameraReelsFill } from "react-icons/bs";

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
    const updatedRooms = await httpManager.getAllRooms();

    setRooms(updatedRooms);
    setDisplayedRooms(updatedRooms);
  }
  
  useEffect(() => {
    dispatch(updatedIsInRoom(false));
    fetchRooms();
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
    navigate(`${ClientEndpoints.room}/${item.roomHash}`, { state: { roomHash: item.roomHash, roomPassword: "" } });
  };

  const handlePrivateRoomListItemClick = (item: Room, password: string) => {
    navigate(`${ClientEndpoints.room}/${item.roomHash}`, { state: { roomHash: item.roomHash, roomPassword: password } });
  }

  return (
    <div className="container">
      <div className="row justify-content-center">
      <div className="main-menu-panel d-block justify-content-center text-center mb-3">
          <h2 className="text-white font-border"><b>Shared Watch</b> <BsFillCameraReelsFill /></h2>
            <span className="text-white" style={{marginRight: "15px"}}>Your username: </span>
              <InputForm
                classNames={"rounded-3"}
                placeholder={"Type here"}
                value={userState.username}
                trim={true}
                onChange={(value: string) => dispatch(updatedUsername(value))}
              />
          </div>
        <div className="main-menu-panel col-xl-6 col-lg-6 col-md-8 col-10 bg-dark py-3 px-5 rounded-4">
          <h3 className="text-white text-center ">Rooms</h3>
          <div className="d-flex justify-content-between align-items-center">
            <div>
              <InputForm
                classNames={"rounded-3"}
                placeholder={"Search room name"}
                value={searchText}
                trim={false}
                onChange={(value: string) => setSearchText(value)}
              />
              <div className="mt-3 mb-3">
                <Switch
                  label={"Show only available rooms"}
                  defaultIsChecked={displayOnlyAvailableRooms as boolean}
                  isEnabled={true}
                  onCheckChange={(value: boolean) => setDisplayOnlyAvailableRooms(value)}
                />
              </div>
            </div>
            <div className="text-center">
              <CreateRoomModal
                title={"Create new room"}
                acceptText="Create"
                declineText="Go back"
              />
            </div>
          </div>
          {
            userState.username.length < 3 &&
            <h6 className="text-info text-center mt-1">Enter username with at least 3 characters to access rooms</h6>
          }
          {
            displayedRooms.length === 0 &&
            <h5 className="text-danger text-center mt-3">No rooms found</h5>
          }
          {
            displayedRooms.length !== 0 &&
            <div className="main-menu-list">
              <RoomList
                list={displayedRooms}
                onPublicRoomClick={handlePublicRoomListItemClick}
                onPrivateRoomClick={handlePrivateRoomListItemClick}
              />
            </div>
          }
        </div>
      </div>
    </div>
  )
}

import { useEffect, useState } from "react";
import { Room } from "../types/Room";
import { RoomTypesEnum } from "../enums/RoomTypesEnum";
import RoomList from "./RoomList";
import { useNavigate } from "react-router-dom";
import { ClientEndpoints } from "../classes/ClientEndpoints";
import MainMenuModal from "./MainMenuModal";
import { InputForm } from "./InputForm";
import Switch from "./Switch";

export default function MainView() {
  const [rooms, setRooms] = useState<Room[]>([]);
  const [displayedRooms, setDisplayedRooms] = useState<Room[]>([]);
  const [displayOnlyAvailableRooms, setDisplayOnlyAvailableRooms] = useState<boolean>(false);
  const [searchText, setSearchText] = useState<string>("");
  const navigate = useNavigate();

  useEffect(() => {
    const room1: Room = {
      hash: "abc",
      roomName: "Test Room 1",
      occupiedSlots: 2,
      totalSlots: 4,
      roomType: RoomTypesEnum.public
    };

    const room2: Room = {
      hash: "bcd",
      roomName: "Test Room 2",
      occupiedSlots: 5,
      totalSlots: 5,
      roomType: RoomTypesEnum.private
    };
 
    const room3: Room = {
      hash: "adr",
      roomName: "Test Room 3",
      occupiedSlots: 1,
      totalSlots: 3,
      roomType: RoomTypesEnum.public
    };

    const room4: Room = {
      hash: "rere",
      roomName: "My roooom",
      occupiedSlots: 2,
      totalSlots: 6,
      roomType: RoomTypesEnum.private
    };

    const room5: Room = {
      hash: "ere",
      roomName: "Cinema",
      occupiedSlots: 6,
      totalSlots: 6,
      roomType: RoomTypesEnum.public
    };

    const room6: Room = {
      hash: "agf",
      roomName: "Cinema 2",
      occupiedSlots: 5,
      totalSlots: 6,
      roomType: RoomTypesEnum.private
    };

    const updatedRooms: Room[] = [room1, room2, room3, room4, room5, room6];

    setRooms(updatedRooms);
    setDisplayedRooms(updatedRooms);
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
    navigate(`${ClientEndpoints.room}/${item.hash}`);
  };

  const handlePrivateRoomListItemClick = (item: Room, password: string) => {
    navigate(`${ClientEndpoints.room}/${item.hash}`);
  }

  const handleModalAcceptButtonClick = () => {
    navigate(`${ClientEndpoints.room}/${1}`);
  };

  return (
<div className="container">
  <div className="row justify-content-center">
    <div className="main-menu-panel col-xl-6 col-lg-6 col-md-8 col-10 bg-dark py-3 px-5 rounded-4">
      <h3 className="text-white text-center">Rooms</h3>
      <div className="d-flex justify-content-between align-items-center">
        <div>
          <InputForm
            classNames={"rounded-3"}
            placeholder={"Search name"}
            value={searchText}
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
        <div>
          <MainMenuModal
            title={"Create new room"}
            acceptText="Create"
            declineText="Go back"
            onAcceptClick={handleModalAcceptButtonClick}
          />  
        </div>
      </div>
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

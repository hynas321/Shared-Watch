import { useEffect, useState } from "react";
import { Room } from "../types/Room";
import { RoomTypesEnum } from "../enums/RoomTypesEnum";
import RoomList from "./RoomList";
import Button from "./Button";
import { useNavigate } from "react-router-dom";
import { ClientEndpoints } from "../classes/ClientEndpoints";

export default function MainView() {
  const [rooms, setRooms] = useState<Room[]>([]);
  const navigate = useNavigate();

  useEffect(() => {
    const room1: Room = {
      id: 1,
      roomName: "Test Room 1",
      hostName: "Peter 1",
      availableSlots: 2,
      totalSlots: 4,
      roomType: RoomTypesEnum.public
    };

    const room2: Room = {
      id: 2,
      roomName: "Test Room 2",
      hostName: "Peter 2",
      availableSlots: 5,
      totalSlots: 5,
      roomType: RoomTypesEnum.private
    };

    const room3: Room = {
      id: 3,
      roomName: "Test Room 3",
      hostName: "Peter 3",
      availableSlots: 1,
      totalSlots: 3,
      roomType: RoomTypesEnum.public
    };

    const updatedRooms: Room[] = [room1, room2, room3];

    setRooms(updatedRooms);
  }, []);

  const handleRoomListItemClick = (item: Room) => {
    navigate(`${ClientEndpoints.room}/${item.id}`);
  };

  const handleCreateRoomButtonClick = () => {
    alert("clicked");
  }

  return (
    <div className="container">
      <div className="row justify-content-center">
        <div className="main-menu-list col-xl-6 col-lg-6 col-md-8 col-10 bg-dark py-3 px-5 rounded-4">
          <h3 className="text-white text-center">{"Available rooms"}</h3>
          <div className="text-end">
            <Button 
              text={"Create new room"}
              classNames="btn btn-success rounded-4"
              styles={{
                marginTop: "5px",
                marginBottom: "10px"
              }}
              onClick={handleCreateRoomButtonClick}
            />
          </div>
          <RoomList
            list={rooms}
            onItemClick={handleRoomListItemClick}
          />
        </div>
      </div>
    </div>
  )
}

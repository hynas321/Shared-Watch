import { useEffect, useState } from "react";
import { Room } from "../types/Room";
import { RoomType } from "../types/RoomType";
import Header from "./Header";
import RoomList from "./RoomList";

export default function MainView() {
  const [rooms, setRooms] = useState<Room[]>([]);

  useEffect(() => {
    const room: Room = {
      id: 1,
      roomName: "Test Room",
      hostName: "Peter",
      availableSlots: 2,
      totalSlots: 4,
      roomType: RoomType.public
    };

    const updatedRooms: Room[] = [room, room, room];
    setRooms(updatedRooms);
  }, []);

  return (
    <div className="container">
      <Header />
      <div className="col-6">
        <RoomList title="Available rooms" list={rooms}></RoomList>
      </div>
    </div>
  )
}

import { BsFillPeopleFill } from "react-icons/bs";
import { Room } from "../types/Room";

export interface RoomListProps {
  list: Room[];
  onItemClick: (item: Room) => void;
}

export default function RoomList({ list, onItemClick }: RoomListProps) {
  const availableRoomStyles = {
    cursor: "pointer"
  }

  const fullRoomStyles = {
    cursor: "not-allowed",
    color: "darkgray"
  }

  return (
    <ul className="list-group rounded-3">
      {
        list.map((room, index) => (
          <li 
            key={index}
            className="list-group-item mt-1 py-2"
            style={room.availableSlots !== room.totalSlots ? availableRoomStyles : fullRoomStyles}
            onClick={room.availableSlots !== room.totalSlots ? () => onItemClick(room) : () => {}}
          >
            <h5 className="d-inline">{room.roomName}</h5>
            <h6><BsFillPeopleFill /> {`${room.availableSlots}/${room.totalSlots}`}</h6>
          </li>
        ))
      }
    </ul>
  );
}
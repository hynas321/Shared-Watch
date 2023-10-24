import { Room } from "../types/Room";

export interface RoomListProps {
  list: Room[];
  onItemClick: (item: Room) => void;
}

export default function RoomList({ list, onItemClick }: RoomListProps) {
  const styles = {
    cursor: "pointer"
  }

  return (
    <ul className="list-group opacity-75 rounded-3">
      {
        list.map((room, index) => (
          <li 
            key={index}
            className="list-group-item mt-1 py-3"
            style={styles}
            onClick={() => onItemClick(room)}
          >
            <h5 className="d-inline">{room.roomName}</h5>
            <h6>{`${room.availableSlots}/${room.totalSlots}`}</h6>
          </li>
        ))
      }
    </ul>
  );
}
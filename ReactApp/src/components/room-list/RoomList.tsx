import { Room } from "../../types/Room";
import RoomListItem from "./RoomListItem";

export interface RoomListProps {
  list: Room[];
  onPublicRoomClick: (room: Room) => void;
  onPrivateRoomClick: (room: Room, password: string) => void;
}

export default function RoomList({ list, onPublicRoomClick, onPrivateRoomClick }: RoomListProps) {
  return (
    <div className="list-group rounded-3 control-panel-list">
      {list.map((room, index) => (
        <RoomListItem
          key={index}
          room={room}
          onPublicRoomClick={onPublicRoomClick}
          onPrivateRoomClick={onPrivateRoomClick}
        />
      ))}
    </div>
  );
}

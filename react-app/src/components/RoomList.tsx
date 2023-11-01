import { BsFillLockFill, BsFillPeopleFill } from "react-icons/bs";
import { Room } from "../types/Room";
import { RoomTypesEnum } from "../enums/RoomTypesEnum";
import { InputForm } from "./InputForm";
import Button from "./Button";
import { useEffect, useState } from "react";

export interface RoomListProps {
  list: Room[];
  onPublicRoomClick: (room: Room) => void;
  onPrivateRoomClick: (room: Room, password: string) => void;
}

export default function RoomList({ list, onPublicRoomClick, onPrivateRoomClick }: RoomListProps) {
  const [privateRoomPassword, setPrivateRoomPassword] = useState<string>("");
  const [isEnterPrivateRoomButtonEnabled, setIsEnterPrivateRoomButtonEnabled] = useState<boolean>(false);

  useEffect(() => {

  }, []);
  const availableRoomStyles = {
    cursor: "pointer"
  }

  const fullRoomStyles = {
    cursor: "not-allowed",
    color: "darkgray"
  }

  return (
    <div className="list-group rounded-3">
      {
        list.map((room, index) => (
          <li
            key={index}
            className="list-group-item mt-1 py-2"
            style={room.occupiedSlots !== room.totalSlots ? availableRoomStyles : fullRoomStyles}
            onClick={(room.occupiedSlots !== room.totalSlots && room.roomType === RoomTypesEnum.public) ? () => onPublicRoomClick(room) : undefined}
            {
              ...room.roomType === RoomTypesEnum.private ? {
                'data-bs-toggle': 'collapse',
                'data-bs-target': `#collapseExample-${room.hash}`,
                'aria-expanded': false
              } : {}
            }
          >
            <h5>{room.roomName} {room.roomType === RoomTypesEnum.private && <BsFillLockFill />}</h5>
            <h6><BsFillPeopleFill /> {`${room.occupiedSlots}/${room.totalSlots}`}</h6>
            {
              (room.roomType === RoomTypesEnum.private && room.occupiedSlots !== room.totalSlots) &&
                <div className="collapse" id={`collapseExample-${room.hash}`}>
                  <div className="card card-body">
                    <span>Provide password to enter the room</span>
                    <InputForm
                      classNames={"form-control"}
                      placeholder={"Type here"}
                      value={privateRoomPassword}
                      onChange={(value: string) => setPrivateRoomPassword(value)}
                    />
                    <Button
                      text={"Enter the room"}
                      classNames={
                        `btn btn-primary ${!isEnterPrivateRoomButtonEnabled && "disabled"}`
                      }
                      onClick={(room: Room) => onPrivateRoomClick(room, privateRoomPassword)}
                    />
                  </div>
                </div>
            }
          </li>
        ))
      }
    </div>
  );
}
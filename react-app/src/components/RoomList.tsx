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
    if (privateRoomPassword.length > 0) {
      setIsEnterPrivateRoomButtonEnabled(true);
    }
    else {
      setIsEnterPrivateRoomButtonEnabled(false);
    }
  }, [privateRoomPassword]);

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
          >
            <div
            {
              ...room.roomType === RoomTypesEnum.private ? {
                'data-bs-toggle': 'collapse',
                'data-bs-target': `#collapseExample-${room.hash}`,
                'aria-expanded': false
              } : {}
            }
            >
              <h5>{room.roomType === RoomTypesEnum.private && <BsFillLockFill />} {room.roomName}</h5>
              <h6><BsFillPeopleFill /> {`${room.occupiedSlots}/${room.totalSlots}`}</h6>
            </div>
            {
              (room.roomType === RoomTypesEnum.private && room.occupiedSlots !== room.totalSlots) &&
                <div className="collapse" id={`collapseExample-${room.hash}`}>
                  <div className="d-flex">
                    <InputForm
                      classNames={"form-control mx-1"}
                      placeholder={"Enter password"}
                      value={privateRoomPassword}
                      onChange={(value: string) => setPrivateRoomPassword(value)}
                    />
                    <Button
                      text={"Enter"}
                      classNames={`btn btn-primary mx-1 ${!isEnterPrivateRoomButtonEnabled && "disabled"}`}
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
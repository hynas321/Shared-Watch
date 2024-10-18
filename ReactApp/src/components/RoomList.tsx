import { BsFillLockFill, BsFillPeopleFill } from "react-icons/bs";
import { Room } from "../types/Room";
import { RoomTypesEnum } from "../enums/RoomTypesEnum";
import { InputField } from "./shared/InputField";
import Button from "./shared/Button";
import { useEffect, useState } from "react";
import { appState } from "../context/AppContext";

export interface RoomListProps {
  list: Room[];
  onPublicRoomClick: (room: Room) => void;
  onPrivateRoomClick: (room: Room, password: string) => void;
}

export default function RoomList({ list, onPublicRoomClick, onPrivateRoomClick }: RoomListProps) {
  const [privateRoomPassword, setPrivateRoomPassword] = useState<string>("");
  const [isEnterPrivateRoomButtonEnabled, setIsEnterPrivateRoomButtonEnabled] =
    useState<boolean>(false);

  useEffect(() => {
    setIsEnterPrivateRoomButtonEnabled(privateRoomPassword.length > 0);
  }, [privateRoomPassword]);

  const isRoomAvailable = (room: Room) => {
    return room.occupiedSlots < room.totalSlots && appState.username.value.length >= 3;
  };

  const renderPrivateRoomControls = (room: Room) => (
    <div className="collapse" id={`collapseExample-${room.roomHash}`}>
      <div className="d-flex">
        <InputField
          classNames="form-control mx-1"
          placeholder="Enter password"
          value={privateRoomPassword}
          trim={true}
          isEnabled={true}
          maxCharacters={35}
          onChange={(value: string) => setPrivateRoomPassword(value)}
          type="password"
        />
        <Button
          text="Enter"
          classNames={`btn btn-primary mx-1 ${!isEnterPrivateRoomButtonEnabled && "disabled"}`}
          onClick={() => onPrivateRoomClick(room, privateRoomPassword)}
        />
      </div>
    </div>
  );

  const renderRoomItem = (room: Room, index: number) => {
    const roomUnavailable =
      room.occupiedSlots === room.totalSlots || appState.username.value.length < 3;
    const roomStyle = roomUnavailable ? { color: "darkgray" } : {};
    const roomClassName = roomUnavailable ? "unavailable-element" : "available-element";

    return (
      <li
        key={index}
        className={`list-group-item mt-1 py-2 ${roomClassName}`}
        style={roomStyle}
        onClick={
          isRoomAvailable(room) && room.roomType === RoomTypesEnum.public
            ? () => onPublicRoomClick(room)
            : undefined
        }
      >
        <div
          {...(room.roomType === RoomTypesEnum.private && {
            "data-bs-toggle": "collapse",
            "data-bs-target": `#collapseExample-${room.roomHash}`,
            "aria-expanded": false,
          })}
        >
          <h5>
            {room.roomType === RoomTypesEnum.private && <BsFillLockFill />} {room.roomName}
          </h5>
          <h6>
            <BsFillPeopleFill /> {`${room.occupiedSlots}/${room.totalSlots}`}
          </h6>
        </div>
        {isRoomAvailable(room) &&
          room.roomType === RoomTypesEnum.private &&
          renderPrivateRoomControls(room)}
      </li>
    );
  };

  return <div className="list-group rounded-3 control-panel-list">{list.map(renderRoomItem)}</div>;
}

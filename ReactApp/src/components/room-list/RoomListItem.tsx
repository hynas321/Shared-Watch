import { BsFillLockFill, BsFillPeopleFill } from "react-icons/bs";
import { Room } from "../../types/Room";
import { RoomTypesEnum } from "../../enums/RoomTypesEnum";
import { InputField } from "../shared/InputField";
import Button from "../shared/Button";
import { useEffect, useState } from "react";
import { appState } from "../../context/AppContext";

interface RoomListItemProps {
  room: Room;
  onPublicRoomClick: (room: Room) => void;
  onPrivateRoomClick: (room: Room, password: string) => void;
}

export default function RoomListItem({
  room,
  onPublicRoomClick,
  onPrivateRoomClick,
}: RoomListItemProps) {
  const [privateRoomPassword, setPrivateRoomPassword] = useState<string>("");
  const [isEnterPrivateRoomButtonEnabled, setIsEnterPrivateRoomButtonEnabled] = useState(false);

  useEffect(() => {
    setIsEnterPrivateRoomButtonEnabled(privateRoomPassword.length > 0);
  }, [privateRoomPassword]);

  const isRoomAvailable = () =>
    room.occupiedSlots < room.totalSlots && appState.username.value.length >= 3;

  const roomUnavailable = !isRoomAvailable();
  const roomStyle = roomUnavailable ? { color: "darkgray" } : {};
  const roomClassName = roomUnavailable ? "unavailable-element" : "available-element";

  const handlePrivateEnter = () => onPrivateRoomClick(room, privateRoomPassword);

  return (
    <li
      className={`list-group-item mt-1 py-2 ${roomClassName}`}
      style={roomStyle}
      onClick={
        isRoomAvailable() && room.roomType === RoomTypesEnum.public
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

      {isRoomAvailable() && room.roomType === RoomTypesEnum.private && (
        <div className="collapse" id={`collapseExample-${room.roomHash}`}>
          <div className="d-flex">
            <InputField
              classNames="form-control mx-1"
              placeholder="Enter password"
              value={privateRoomPassword}
              trim={true}
              isEnabled={true}
              maxCharacters={35}
              onChange={setPrivateRoomPassword}
              type="password"
            />
            <Button
              text="Enter"
              classNames={`btn btn-primary mx-1 ${
                !isEnterPrivateRoomButtonEnabled && "disabled"
              }`}
              onClick={handlePrivateEnter}
            />
          </div>
        </div>
      )}
    </li>
  );
}

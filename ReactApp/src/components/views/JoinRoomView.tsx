import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useSignal } from "@preact/signals-react";
import { animated, useSpring } from "@react-spring/web";
import { ToastContainer, toast } from "react-toastify";
import { BsFillLockFill, BsFillPeopleFill, BsFillPersonLinesFill } from "react-icons/bs";
import { HttpStatusCode } from "axios";
import { ping } from "ldrs";
import Header from "../Header";
import { InputField } from "../shared/InputField";
import Button from "../shared/Button";
import { appState } from "../../context/AppContext";
import { HttpService } from "../../classes/services/HttpService";
import { HttpUrlHelper } from "../../classes/helpers/HttpUrlHelper";
import { RoomHelper } from "../../classes/helpers/RoomHelper";
import { ClientEndpoints } from "../../classes/constants/ClientEndpoints";
import { RoomTypesEnum } from "../../enums/RoomTypesEnum";
import { ToastNotificationEnum } from "../../enums/ToastNotificationEnum";
import { Room } from "../../types/Room";
import { RoomState } from "../../types/RoomState";

export default function JoinRoomView() {
  const navigate = useNavigate();

  const [privateRoomPassword, setPrivateRoomPassword] = useState("");
  const [isEnterPrivateRoomButtonEnabled, setIsEnterPrivateRoomButtonEnabled] = useState(false);
  const [isRoomLoading, setIsRoomLoading] = useState(true);

  const room = useSignal<Room>({
    roomHash: "",
    roomName: "-",
    roomType: RoomTypesEnum.public,
    occupiedSlots: 0,
    totalSlots: 0,
  });

  const roomHelper = RoomHelper.getInstance();
  const httpService = HttpService.getInstance();
  const httpUrlHelper = new HttpUrlHelper();

  const isUsernameValid = appState.username.value.length >= 3;
  const isRoomFull = room.value.occupiedSlots >= room.value.totalSlots;
  const isPublicRoom = room.value.roomType === RoomTypesEnum.public;
  const isPrivateRoom = room.value.roomType === RoomTypesEnum.private;

  useEffect(() => {
    setIsEnterPrivateRoomButtonEnabled(privateRoomPassword.length > 0);
  }, [privateRoomPassword]);

  useEffect(() => {
    ping.register();
    initializeView();
  }, []);

  const initializeView = async () => {
    const hash = httpUrlHelper.getRoomHash(window.location.href);
    const [responseStatus, responseData] = await httpService.getRoom(hash);

    if (responseStatus !== HttpStatusCode.Ok) {
      toast.error("Room not found", {
        containerId: ToastNotificationEnum.Main,
      });
      navigate(ClientEndpoints.mainMenu, { replace: true });
      return;
    }

    setTimeout(() => {
      setIsRoomLoading(false);
    }, 550);

    room.value = responseData as Room;
    appState.roomType.value = room.value.roomType;
    appState.isInRoom.value = false;
  };

  const springs = useSpring({
    from: { y: 400 },
    to: { y: 0 },
    config: {
      mass: 1.75,
      tension: 150,
      friction: 20,
    },
  });

  const navigateToRoom = (roomHash: string) => {
    navigate(`${ClientEndpoints.room}/${roomHash}`, { replace: true });
  };

  const handlePublicRoomClick = async () => {
    const roomState: RoomState = {
      roomHash: room.value.roomHash,
      roomName: room.value.roomName,
      roomPassword: "",
    };

    const canJoin = await roomHelper.joinRoom(roomState);

    if (canJoin) {
      navigateToRoom(roomState.roomHash);
    }
  };

  const handlePrivateRoomClick = async () => {
    const roomState: RoomState = {
      roomHash: room.value.roomHash,
      roomName: room.value.roomName,
      roomPassword: privateRoomPassword,
    };

    const canJoin = await roomHelper.joinRoom(roomState);

    if (canJoin) {
      navigateToRoom(roomState.roomHash);
    }
  };

  const renderContent = () => {
    if (isRoomLoading) {
      return (
        <h1
          className="text-white text-center"
          style={{ marginTop: "2rem", marginBottom: "1.2rem" }}
        >
          <l-ping size="100" speed="1.25" color="white"></l-ping>
        </h1>
      );
    }

    if (!isUsernameValid) {
      return (
        <div className="mt-3 mb-4">
          <h1 className="text-white text-center">
            <BsFillPersonLinesFill />
          </h1>
          <h5 className="text-white text-center">Enter your username</h5>
          <h6 className="text-white text-center">To gain access to the room</h6>
        </div>
      );
    }

    const roomItem = (
      <div
        className="list-group-item py-2 available-element"
        style={{ marginTop: "2rem", marginBottom: "2.9rem" }}
        onClick={!isRoomFull && isPublicRoom ? handlePublicRoomClick : undefined}
      >
        <div
          {...(isPrivateRoom
            ? {
                "data-bs-toggle": "collapse",
                "data-bs-target": `#collapseExample-${room.value.roomHash}`,
                "aria-expanded": false,
              }
            : {})}
        >
          <h5>
            {isPrivateRoom && <BsFillLockFill />} {room.value.roomName}
          </h5>
          <h6>
            <BsFillPeopleFill /> {`${room.value.occupiedSlots}/${room.value.totalSlots}`}
          </h6>
        </div>
        {!isRoomFull && isPrivateRoom && (
          <div className="collapse" id={`collapseExample-${room.value.roomHash}`}>
            <div className="d-flex">
              <InputField
                classNames="form-control mx-1"
                placeholder="Enter password"
                value={privateRoomPassword}
                trim={true}
                isEnabled={true}
                maxCharacters={35}
                onChange={setPrivateRoomPassword}
              />
              <Button
                text="Enter"
                classNames={`btn btn-primary mx-1 ${
                  !isEnterPrivateRoomButtonEnabled && "disabled"
                }`}
                onClick={handlePrivateRoomClick}
              />
            </div>
          </div>
        )}
      </div>
    );

    return roomItem;
  };

  return (
    <>
      <Header />
      <ToastContainer
        position="top-right"
        autoClose={3000}
        hideProgressBar
        closeOnClick
        draggable
        pauseOnHover={false}
        theme="dark"
        style={{ opacity: 0.9 }}
      />
      <div className="container">
        <div className="row justify-content-center">
          <animated.div
            className="mt-3 col-xl-6 col-lg-6 col-md-8 col-10 bg-dark bg-opacity-50 py-3 px-5 rounded-4"
            style={springs}
          >
            <h3 className="text-white text-center mt-3 mb-3">Join the room</h3>
            <div className="row d-flex justify-content-center align-items-center">
              <div className="list-group rounded-3" style={{ paddingRight: 0 }}>
                {renderContent()}
              </div>
            </div>
            <div className="d-flex justify-content-center">
              <Button
                text="Go to Main Menu"
                classNames="btn btn-primary mx-1 mt-2 mb-4"
                onClick={() => navigate(ClientEndpoints.mainMenu, { replace: true })}
              />
            </div>
          </animated.div>
        </div>
      </div>
    </>
  );
}

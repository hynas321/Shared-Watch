import { useNavigate } from "react-router-dom";
import { useEffect, useContext, useState } from "react";
import { HttpManager } from "../../classes/HttpManager";
import { HttpStatusCodes } from "../../classes/HttpStatusCodes";
import { ClientEndpoints } from "../../classes/ClientEndpoints";
import { AppStateContext } from "../../context/AppContext";
import { RoomTypesEnum } from "../../enums/RoomTypesEnum";
import { animated, useSpring } from "@react-spring/web";
import Header from "../Header";
import { ToastContainer, toast } from "react-toastify";
import { HttpUrlHelper } from "../../classes/HttpUrlHelper";
import { useSignal } from "@preact/signals-react";
import { Room } from "../../types/Room";
import { BsFillLockFill, BsFillPeopleFill, BsFillPersonLinesFill } from "react-icons/bs";
import { InputField } from "../InputField";
import Button from "../Button";
import { RoomState } from "../../types/RoomState";
import { RoomHelper } from "../../classes/RoomHelper";
import { ToastNotificationEnum } from "../../enums/ToastNotificationEnum";
import { ping } from "ldrs";

export default function JoinRoomView() {
  const appState = useContext(AppStateContext);
  const navigate = useNavigate()

  const [privateRoomPassword, setPrivateRoomPassword] = useState<string>("");
  const [isEnterPrivateRoomButtonEnabled, setIsEnterPrivateRoomButtonEnabled] = useState<boolean>(false);
  const [isRoomLoading, setIsRoomLoading] = useState<boolean>(true);

  const room = useSignal<Room>({
    roomHash: "",
    roomName: "-",
    roomType: RoomTypesEnum.public,
    occupiedSlots: 0,
    totalSlots: 0
  });

  const roomHelper = RoomHelper.getInstance();

  useEffect(() => {
    if (privateRoomPassword.length > 0) {
      setIsEnterPrivateRoomButtonEnabled(true);
    }
    else {
      setIsEnterPrivateRoomButtonEnabled(false);
    }
  }, [privateRoomPassword]);


  const httpManager = new HttpManager();
  const httpUrlHelper = new HttpUrlHelper();

  const initializeView = async () => {
    const hash = httpUrlHelper.getRoomHash(window.location.href);
    const [responseStatus, responseData] = await httpManager.getRoom(hash);

    if (responseStatus !== HttpStatusCodes.OK) {
      toast.error(
        "Room not found", {
          containerId: ToastNotificationEnum.Main
        }
      );
      navigate(`${ClientEndpoints.mainMenu}`, { replace: true });
      return;
    }

    setIsRoomLoading(false);

    room.value = responseData as Room;
  
    appState.roomType.value = responseData?.roomType as RoomTypesEnum;
    appState.isInRoom.value = false;
  }

  useEffect(() => {
    ping.register();
    initializeView();
  }, []);

  const springs = useSpring({
    from: { y: 400 },
    to: { y: 0 },
    config: {
      mass: 1.75,
      tension: 150,
      friction: 20,
    }
  });

  const handlePublicRoomListItemClick = async (item: Room) => {
    const roomState: RoomState = {
      roomHash: item.roomHash,
      roomName: item.roomName,
      password: ""
    };

    const canJoin = await roomHelper.joinRoom(roomState);

    if (canJoin) {
      navigate(`${ClientEndpoints.room}/${roomState.roomHash}`, { replace: true });
    }
  };

  const handlePrivateRoomListItemClick = async (item: Room, password: string) => {
    const roomState: RoomState = {
      roomHash: item.roomHash,
      roomName: item.roomName,
      password: password
    };

    const canJoin = await roomHelper.joinRoom(roomState);

    if (canJoin) {
      navigate(`${ClientEndpoints.room}/${roomState.roomHash}`, { replace: true });
    }
  }

  return (
    <>
      <Header />
      <ToastContainer
        position="top-right"
        autoClose={3000}
        hideProgressBar={true}
        closeOnClick={true}
        draggable={true}
        pauseOnHover={false}
        theme="dark"
        style={{opacity: 0.9}}
      />
      <div className="container">
        <div className="row justify-content-center">
          <animated.div className="mt-3 col-xl-6 col-lg-6 col-md-8 col-10 bg-dark bg-opacity-50 py-3 px-5 rounded-4" style={{...springs}}>
            <h3 className="text-white text-center mt-3 mb-3">Join the room</h3>
            <div className="row d-flex justify-content-between align-items-center">
              <div className="list-group rounded-3">
                {
                  (appState.username.value.length < 3 && isRoomLoading === false) &&
                  <div className="mt-3 mb-4">
                    <h1 className="text-white text-center"><BsFillPersonLinesFill /></h1>
                    <h5 className="text-white text-center">Enter your username</h5>
                    <h6 className="text-white text-center">To gain access to the room</h6>
                  </div>
                }
                {
                  (isRoomLoading === true) &&
                  <h1 className="text-white text-center" style={{marginTop: "2rem", marginBottom: "1.2rem"}}>
                    <l-ping
                      size="100"
                      speed="1.25" 
                      color="white" 
                    ></l-ping>
                  </h1>
                }
                {
                  (appState.username.value.length >= 3 && isRoomLoading === false) &&
                  <div
                    className="list-group-item py-2 "
                    style={{marginTop: "2rem", marginBottom: "2.9rem"}}
                    onClick={(room.value.occupiedSlots !== room.value.totalSlots && room.value.roomType === RoomTypesEnum.public && appState.username.value.length >= 3) ? () => handlePublicRoomListItemClick(room.value) : undefined}
                  >
                    <div
                    {
                      ...room.value.roomType === RoomTypesEnum.private ? {
                        'data-bs-toggle': 'collapse',
                        'data-bs-target': `#collapseExample-${room.value.roomHash}`,
                        'aria-expanded': false
                      } : {}
                    }
                    >
                      <h5>{room.value.roomType === RoomTypesEnum.private && <BsFillLockFill />} {room.value.roomName}</h5>
                      <h6><BsFillPeopleFill /> {`${room.value.occupiedSlots}/${room.value.totalSlots}`}</h6>
                    </div>
                    {
                      (room.value.occupiedSlots !== room.value.totalSlots && room.value.roomType === RoomTypesEnum.private && appState.username.value.length >= 3) &&
                      <div className="collapse" id={`collapseExample-${room.value.roomHash}`}>
                        <div className="d-flex">
                          <InputField
                            classNames={"form-control mx-1"}
                            placeholder={"Enter password"}
                            value={privateRoomPassword}
                            trim={true}
                            isEnabled={true}
                            maxCharacters={35}
                            onChange={(value: string) => setPrivateRoomPassword(value)}
                          />
                          <Button
                            text={"Enter"}
                            classNames={`btn btn-primary mx-1 ${!isEnterPrivateRoomButtonEnabled && "disabled"}`}
                            onClick={() => handlePrivateRoomListItemClick(room.value, privateRoomPassword)}
                          />
                        </div>
                      </div>
                    }
                  </div>
                }
              </div>
            </div>
            <div className="d-flex justify-content-center">
              <Button
                text={"Go to Main Menu"}
                classNames={`btn btn-primary mx-1 mt-2 mb-4`}
                onClick={() => navigate(`${ClientEndpoints.mainMenu}`, { replace: true })}
              />
            </div>
          </animated.div>
        </div>
      </div>
    </>
  )
}

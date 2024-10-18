import { useContext, useEffect, useState } from "react";
import { Room } from "../../types/Room";
import RoomList from "../RoomList";
import { useNavigate } from "react-router-dom";
import { ClientEndpoints } from "../../classes/constants/ClientEndpoints";
import { HttpService } from "../../classes/services/HttpService";
import { ToastContainer, toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import { RoomState } from "../../types/RoomState";
import { animated, useSpring } from "@react-spring/web";
import { AppStateContext } from "../../context/AppContext";
import { RoomHelper } from "../../classes/helpers/RoomHelper";
import {
  BsDoorOpenFill,
  BsExclamationTriangleFill,
  BsFillCameraReelsFill,
  BsFillPersonLinesFill,
} from "react-icons/bs";
import { ToastNotificationEnum } from "../../enums/ToastNotificationEnum";
import { HttpStatusCode } from "axios";
import { helix } from "ldrs";
import { SessionStorageService } from "../../classes/services/SessionStorageService";
import { InputField } from "../shared/InputField";
import CreateRoomModal from "../CreateRoomModal";

const EmptyRoomsMessage = () => (
  <>
    <h1 className="text-white text-center" style={{ marginTop: "7rem" }}>
      <BsDoorOpenFill />
    </h1>
    <h5 className="text-white text-center">No rooms to display</h5>
  </>
);

const UsernamePrompt = () => (
  <>
    <h1 className="text-white text-center" style={{ marginTop: "7rem" }}>
      <BsFillPersonLinesFill />
    </h1>
    <h5 className="text-white text-center">Enter your username</h5>
    <h6 className="text-white text-center">To gain access to room functionalities</h6>
  </>
);

const ConnectionError = () => (
  <>
    <h1 className="text-white text-center" style={{ marginTop: "7rem" }}>
      <BsExclamationTriangleFill />
    </h1>
    <h5 className="text-white text-center">Connection Issue</h5>
    <h6 className="text-white text-center">Check your internet connection or refresh the page</h6>
  </>
);

const LoadingRooms = () => (
  <h1 className="text-white text-center" style={{ marginTop: "7rem" }}>
    <l-helix size="100" speed="1.25" color="white" />
  </h1>
);

export default function MainView() {
  const appState = useContext(AppStateContext);
  const navigate = useNavigate();
  const [displayedRooms, setDisplayedRooms] = useState<Room[]>([]);
  const [areRoomsFetched, setAreRoomsFetched] = useState<boolean>(false);
  const [fetchError, setFetchError] = useState<boolean>(false);

  const httpService = HttpService.getInstance();
  const roomHelper = RoomHelper.getInstance();
  const sessionStorageService = SessionStorageService.getInstance();

  const springs = useSpring({
    from: { y: 400 },
    to: { y: 0 },
    config: { mass: 1.75, tension: 150, friction: 20 },
  });

  useEffect(() => {
    const initialize = async () => {
      helix.register();
      fetchRooms();
      const username = sessionStorageService.getUsername();
      if (username) appState.username.value = username;
    };
    initialize();
  }, [appState.username]);

  const fetchRooms = async () => {
    setTimeout(async () => {
      const [responseStatusCode, responseData] = await httpService.getAllRooms();
      if (responseStatusCode !== HttpStatusCode.Ok) {
        toast.error("Could not load the rooms", { containerId: ToastNotificationEnum.Main });
        setFetchError(true);
      } else {
        setDisplayedRooms(responseData ?? []);
        setFetchError(false);
      }
      setAreRoomsFetched(true);
    }, 500);
  };

  const handleRoomClick = async (room: Room, password: string = "") => {
    const roomState: RoomState = {
      roomHash: room.roomHash,
      roomName: room.roomName,
      roomPassword: password,
    };
    if (await roomHelper.joinRoom(roomState))
      navigate(`${ClientEndpoints.room}/${roomState.roomHash}`, { replace: true });
  };

  return (
    <>
      <ToastContainer
        containerId={ToastNotificationEnum.Main}
        position="top-right"
        autoClose={3000}
        hideProgressBar
        closeOnClick
        draggable
        pauseOnHover={false}
        theme="dark"
        style={{ top: "0px", opacity: 0.9 }}
      />
      <div className="container">
        <div className="row justify-content-center">
          <animated.div
            className="main-menu-panel mt-5 col-xl-6 col-lg-7 col-md-9 col-11 bg-dark bg-opacity-50 py-3 px-5 rounded-4"
            style={springs}
          >
            <h3 className="text-white text-center mt-4 mb-4">
              <i>
                <b>Shared Watch</b>
              </i>{" "}
              <BsFillCameraReelsFill />
            </h3>

            <div className="row justify-content-center align-items-center">
              <div className="col-md-8 col-sm-12 mt-3">
                <InputField
                  classNames={`form-control form-control rounded-3 ${
                    appState.username.value.length < 3 ? "is-invalid" : "is-valid"
                  }`}
                  placeholder="Username (min. 3 characters)"
                  value={appState.username.value}
                  trim={true}
                  maxCharacters={25}
                  isEnabled={true}
                  onChange={(value: string) => {
                    appState.username.value = value;
                    sessionStorageService.setUsername(value);
                  }}
                />
              </div>
              <div className="col-md-4 col-sm-12 text-md-end text-center mt-3">
                <CreateRoomModal
                  acceptText="Create"
                  declineText="Go back"
                  isEnabled={appState.username.value.length >= 3}
                />
              </div>
            </div>

            {!areRoomsFetched ? (
              <LoadingRooms />
            ) : fetchError ? (
              <ConnectionError />
            ) : (
              <>
                {appState.username.value.length < 3 ? (
                  <UsernamePrompt />
                ) : displayedRooms.length === 0 ? (
                  <EmptyRoomsMessage />
                ) : (
                  <div className="mt-5">
                    <RoomList
                      list={displayedRooms}
                      onPublicRoomClick={handleRoomClick}
                      onPrivateRoomClick={handleRoomClick}
                    />
                  </div>
                )}
              </>
            )}
          </animated.div>
        </div>
      </div>
    </>
  );
}

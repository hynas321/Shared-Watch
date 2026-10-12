import { useContext, useEffect, useMemo, useState } from "react";
import { Room } from "../../types/Room";
import RoomList from "../room-list/RoomList";
import { useNavigate } from "react-router-dom";
import { ClientEndpoints } from "../../classes/constants/ClientEndpoints";
import { ToastContainer, toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import { RoomState } from "../../types/RoomState";
import { animated, useSpring } from "@react-spring/web";
import { AppStateContext } from "../../context/AppContext";
import {
  BsDoorOpenFill,
  BsExclamationTriangleFill,
  BsFillCameraReelsFill,
  BsFillPersonLinesFill,
  BsSearch,
} from "react-icons/bs";
import { ToastNotificationEnum } from "../../enums/ToastNotificationEnum";
import { HttpStatusCode } from "axios";
import { helix } from "ldrs";
import { InputField } from "../shared/InputField";
import CreateRoomModal from "../create-room-modal/CreateRoomModal";
import { useRoomJoin } from "../../hooks/useRoomJoin";
import { useSessionStorage } from "../../hooks/useSessionStorage";
import api from "../../classes/Http/Api";
import BackgroundSelector from "../shared/BackgroundSelector";

const EmptyRoomsMessage = () => (
  <>
    <h1 className="text-white text-center" style={{ marginTop: "7rem" }}>
      <BsDoorOpenFill />
    </h1>
    <h5 className="text-white text-center">No rooms found</h5>
    <h6 className="text-white-50 text-center">Create a room to get started, or check back later.</h6>
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
    <h5 className="text-white text-center">Could not connect to the server</h5>
    <h6 className="text-white text-center">
      The room list could not be loaded. Check your connection and try again.
    </h6>
  </>
);

const LoadingRooms = () => (
  <h1 className="text-white text-center" style={{ marginTop: "7rem" }}>
    <l-helix size="100" speed="1.25" color="white" />
  </h1>
);

const NoMatchingRoomsMessage = () => (
  <>
    <h1 className="text-white text-center" style={{ marginTop: "7rem" }}>
      <BsSearch />
    </h1>
    <h5 className="text-white text-center">No rooms found for this search</h5>
    <h6 className="text-white-50 text-center">Try a different room name or clear the search.</h6>
  </>
);

export default function MainView() {
  const appState = useContext(AppStateContext);
  const navigate = useNavigate();

  const { joinRoom } = useRoomJoin();
  const { username, setUsername } = useSessionStorage();

  const [displayedRooms, setDisplayedRooms] = useState<Room[]>([]);
  const [roomSearch, setRoomSearch] = useState("");
  const [areRoomsFetched, setAreRoomsFetched] = useState<boolean>(false);
  const [fetchError, setFetchError] = useState<boolean>(false);
  const backendAvailable = areRoomsFetched && !fetchError;

  const filteredRooms = useMemo(() => {
    const search = roomSearch.trim().toLocaleLowerCase();
    if (!search) {
      return displayedRooms;
    }

    return displayedRooms.filter((room) =>
      room.roomName.toLocaleLowerCase().includes(search)
    );
  }, [displayedRooms, roomSearch]);

  const springs = useSpring({
    from: { y: 400 },
    to: { y: 0 },
    config: { mass: 1.75, tension: 150, friction: 20 },
  });

  useEffect(() => {
    const disconnectMessage = sessionStorage.getItem("backendDisconnectMessage");
    if (disconnectMessage) {
      sessionStorage.removeItem("backendDisconnectMessage");
      toast.error(disconnectMessage, { containerId: ToastNotificationEnum.Main });
    }

    helix.register();
    void fetchRooms();

    const refreshRooms = () => {
      void fetchRooms();
    };
    window.addEventListener("shared-watch:room-changed", refreshRooms);
    window.addEventListener("focus", refreshRooms);

    if (username) {
      appState.username.value = username;
    }

    return () => {
      window.removeEventListener("shared-watch:room-changed", refreshRooms);
      window.removeEventListener("focus", refreshRooms);
    };
  }, [appState.username, username]);

  const fetchRooms = async () => {
    const [responseStatusCode, responseData] = await api.getAllRooms();
    if (responseStatusCode !== HttpStatusCode.Ok) {
      await api.healthCheck();
      setDisplayedRooms([]);
      setFetchError(true);
    } else {
      setDisplayedRooms(responseData ?? []);
      setFetchError(false);
    }
    setAreRoomsFetched(true);
  };

  const handleRoomClick = async (room: Room, password: string = "") => {
    const roomState: RoomState = {
      roomHash: room.roomHash,
      roomName: room.roomName,
      roomPassword: password,
    };
    if (await joinRoom(roomState))
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
      <div className="container main-menu-layout">
        <div className="row justify-content-center">
          <animated.div
            className="main-menu-panel mt-3 col-xl-6 col-lg-7 col-md-9 col-11 bg-dark bg-opacity-50 py-3 px-5 rounded-4"
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
                    setUsername(value);
                  }}
                />
              </div>
              <div className="col-md-4 col-sm-12 text-md-end text-center mt-3">
                <CreateRoomModal
                  acceptText="Create"
                  declineText="Go back"
                  isEnabled={backendAvailable && appState.username.value.length >= 3}
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
                    <InputField
                      classNames="form-control rounded-3 mb-3"
                      placeholder="Search rooms by name"
                      value={roomSearch}
                      trim={false}
                      maxCharacters={55}
                      isEnabled={true}
                      onChange={setRoomSearch}
                    />
                    {filteredRooms.length === 0 ? (
                      <NoMatchingRoomsMessage />
                    ) : (
                    <RoomList
                      list={filteredRooms}
                      onPublicRoomClick={handleRoomClick}
                      onPrivateRoomClick={handleRoomClick}
                    />
                    )}
                  </div>
                )}
              </>
            )}
          </animated.div>
        </div>
        <div className="main-background-selector">
          <BackgroundSelector />
        </div>
      </div>
    </>
  );
}

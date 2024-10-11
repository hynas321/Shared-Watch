import { useContext, useEffect, useState, useCallback } from "react";
import { Room } from "../../types/Room";
import RoomList from "../RoomList";
import { useNavigate } from "react-router-dom";
import { ClientEndpoints } from "../../classes/constants/ClientEndpoints";
import { HttpService } from "../../classes/services/HttpService";
import Header from "../Header";
import { ToastContainer, toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import { RoomState } from "../../types/RoomState";
import { animated, useSpring } from "@react-spring/web";
import { AppStateContext } from "../../context/AppContext";
import { RoomHelper } from "../../classes/helpers/RoomHelper";
import { BsDoorOpenFill, BsExclamationTriangleFill, BsFillPersonLinesFill } from "react-icons/bs";
import { ToastNotificationEnum } from "../../enums/ToastNotificationEnum";
import { HttpStatusCode } from "axios";
import RoomFilters from "../RoomFilters";
import { helix } from "ldrs";
import { SessionStorageService } from "../../classes/services/SessionStorageService";

export default function MainView() {
  const appState = useContext(AppStateContext);
  const navigate = useNavigate();

  const [rooms, setRooms] = useState<Room[]>([]);
  const [displayedRooms, setDisplayedRooms] = useState<Room[]>([]);
  const [displayOnlyAvailableRooms, setDisplayOnlyAvailableRooms] = useState<boolean>(false);
  const [searchText, setSearchText] = useState<string>("");
  const [areRoomsFetched, setAreRoomsFetched] = useState<boolean>(false);
  const [fetchError, setFetchError] = useState<boolean>(false);

  const httpService = HttpService.getInstance();
  const roomHelper = RoomHelper.getInstance();
  const sessionStorageService = SessionStorageService.getInstance();

  useEffect(() => {
    const initialize = async () => {
      await initializeAppState();
    };
    initialize();
  }, []);

  const springs = useSpring({
    from: { y: 400 },
    to: { y: 0 },
    config: { mass: 1.75, tension: 150, friction: 20 },
  });

  const initializeAppState = async () => {
    helix.register();
    fetchRooms();

    const username = sessionStorageService.getUsername();

    if (username) {
      appState.username.value = username;
    }
  };

  const fetchRooms = () => {
    setTimeout(async () => {
      const [responseStatusCode, responseData] = await httpService.getAllRooms();

      if (responseStatusCode !== HttpStatusCode.Ok) {
        toast.error("Could not load the rooms", { containerId: ToastNotificationEnum.Main });
        setFetchError(true);
        setAreRoomsFetched(true);
      } else {
        setRooms(responseData ?? []);
        setDisplayedRooms(responseData ?? []);
        setFetchError(false);
        setAreRoomsFetched(true);
      }
    }, 500);

    resetSearch();
  };

  const resetSearch = () => {
    setSearchText(".");
    setSearchText("");
  };

  const filterRooms = useCallback(() => {
    const filterLogic = (room: Room) =>
      room.roomName.toLowerCase().includes(searchText.toLowerCase()) &&
      (!displayOnlyAvailableRooms || room.occupiedSlots !== room.totalSlots);

    setDisplayedRooms(rooms.filter(filterLogic));
  }, [searchText, rooms, displayOnlyAvailableRooms]);

  useEffect(() => {
    filterRooms();
  }, [searchText, displayOnlyAvailableRooms, rooms, filterRooms]);

  const handleRoomClick = async (room: Room, password: string = "") => {
    const roomState: RoomState = {
      roomHash: room.roomHash,
      roomName: room.roomName,
      roomPassword: password,
    };
    if (await roomHelper.joinRoom(roomState)) {
      navigate(`${ClientEndpoints.room}/${roomState.roomHash}`, { replace: true });
    }
  };

  const renderEmptyRoomsMessage = () => (
    <>
      <h1 className="text-white text-center" style={{ marginTop: "4rem" }}>
        <BsDoorOpenFill />
      </h1>
      <h5 className="text-white text-center">No rooms to display</h5>
    </>
  );

  const renderUsernamePrompt = () => (
    <>
      <h1 className="text-white text-center" style={{ marginTop: "9rem" }}>
        <BsFillPersonLinesFill />
      </h1>
      <h5 className="text-white text-center">Enter your username</h5>
      <h6 className="text-white text-center">To gain access to room functionalities</h6>
    </>
  );

  const renderConnectionError = () => (
    <>
      <h1 className="text-white text-center" style={{ marginTop: "9rem" }}>
        <BsExclamationTriangleFill />
      </h1>
      <h5 className="text-white text-center">Connection Issue</h5>
      <h6 className="text-white text-center">Check your internet connection or refresh the page</h6>
    </>
  );

  const renderLoadingRooms = () => (
    <h1 className="text-white text-center" style={{ marginTop: "9rem" }}>
      <l-helix size="100" speed="1.25" color="white" />
    </h1>
  );

  return (
    <>
      <Header />
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
            className="main-menu-panel mt-3 col-xl-6 col-lg-7 col-md-9 col-11 bg-dark bg-opacity-50 py-3 px-5 rounded-4"
            style={springs}
          >
            <h3 className="text-white text-center mt-3 mb-3">Rooms</h3>
            {!areRoomsFetched ? (
              renderLoadingRooms()
            ) : fetchError ? (
              renderConnectionError()
            ) : (
              <>
                {appState.username.value.length < 3 ? (
                  renderUsernamePrompt()
                ) : (
                  <>
                    <RoomFilters
                      searchText={searchText}
                      onSearchTextChange={setSearchText}
                      displayOnlyAvailableRooms={displayOnlyAvailableRooms}
                      onDisplayOnlyAvailableRoomsChange={setDisplayOnlyAvailableRooms}
                    />
                    {displayedRooms.length === 0 ? (
                      renderEmptyRoomsMessage()
                    ) : (
                      <RoomList
                        list={displayedRooms}
                        onPublicRoomClick={(room) => handleRoomClick(room)}
                        onPrivateRoomClick={handleRoomClick}
                      />
                    )}
                  </>
                )}
              </>
            )}
          </animated.div>
        </div>
      </div>
    </>
  );
}

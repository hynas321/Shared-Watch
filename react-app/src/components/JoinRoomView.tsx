import { useLocation, useNavigate } from "react-router-dom";
import { useEffect, useContext } from "react";
import { HttpManager } from "../classes/HttpManager";
import { HttpStatusCodes } from "../classes/HttpStatusCodes";
import { ClientEndpoints } from "../classes/ClientEndpoints";
import { AppStateContext } from "../context/RoomHubContext";
import { RoomTypesEnum } from "../enums/RoomTypesEnum";

export default function JoinRoomView() {
  const appState = useContext(AppStateContext);
  const location = useLocation();
  const navigate = useNavigate()

  const httpManager = new HttpManager();

  const initializeView = async () => {
    if (!location.state) {
      const [responseStatus, responseData] = await httpManager.checkIfRoomExists(window.location.href);

      if (responseStatus !== HttpStatusCodes.OK) {
        navigate(`${ClientEndpoints.mainMenu}`, { replace: true });
        return;
      }

      appState.roomType.value = responseData?.roomType as RoomTypesEnum;
    }
  }

  useEffect(() => {
    initializeView();
  }, []);

  return (
    <div>JoinRoomView {appState.roomType.value}</div>
  )
}

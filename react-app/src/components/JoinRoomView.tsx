import { useLocation, useNavigate } from "react-router-dom";
import { JoinRoomNavigationState } from "../types/JoinRoomNavigationState";
import { useEffect, useState } from "react";
import { HttpManager } from "../classes/HttpManager";
import { HttpStatusCodes } from "../classes/HttpStatusCodes";
import { ClientEndpoints } from "../classes/ClientEndpoints";
import { RoomTypesEnum } from "../enums/RoomTypesEnum";

export default function JoinRoomView() {
  const location = useLocation();
  const navigate = useNavigate();
  const { ...joinRoomNavigationState }: JoinRoomNavigationState = location.state ?? location.state === null;

  const [roomType, setRoomType] = useState<RoomTypesEnum>(RoomTypesEnum.public);

  const httpManager = new HttpManager();

  const initializeView = async () => {
    if (!location.state) {
      const [responseStatus, responseData] = await httpManager.checkIfRoomExists(window.location.href);
      console.log(responseStatus);
      if (responseStatus !== HttpStatusCodes.OK) {
        navigate(`${ClientEndpoints.mainMenu}`, { replace: true });
        return;
      }

      setRoomType(responseData?.roomType as RoomTypesEnum);
    }
  }

  useEffect(() => {
    initializeView();
  }, []);

  return (
    <div>JoinRoomView {location.state === null ? `Type: ${roomType}` : `Type: ${joinRoomNavigationState.roomType}`}</div>
  )
}

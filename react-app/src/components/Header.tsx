import { BsDoorOpenFill, BsFillCameraReelsFill } from "react-icons/bs";
import { InputForm } from "./InputForm";
import { useAppSelector } from "../redux/hooks";
import { useDispatch } from "react-redux";
import { updatedIsInRoom, updatedUsername } from "../redux/slices/userState-slice";
import Button from "./Button";
import { useNavigate } from "react-router-dom";
import { ClientEndpoints } from "../classes/ClientEndpoints";
import { HttpManager } from "../classes/HttpManager";
import { updatedRoomHash, updatedRoomName, updatedRoomType } from "../redux/slices/roomState-slice";
import { RoomTypesEnum } from "../enums/RoomTypesEnum";

export default function Header() {
  const userState = useAppSelector((state) => state.userState);
  const roomState = useAppSelector((state) => state.roomState);
  const dispatch = useDispatch();
  const navigate = useNavigate();

  const httpManager = new HttpManager();

  const handleLeaveRoomButtonClick = async () => {
    dispatch(updatedRoomHash(""));
    dispatch(updatedRoomName(""));
    dispatch(updatedRoomType(RoomTypesEnum.public));
    dispatch(updatedIsInRoom(false));

    navigate(ClientEndpoints.mainMenu); 
    
    await httpManager.leaveRoom(roomState.roomHash);
  }

  return (
    <nav className="navbar navbar-dark bg-dark mb-3">
      <div className="d-flex align-items-center">
        <a className="navbar-brand ms-3" href="/"><i><b>SharedWatch</b></i> <BsFillCameraReelsFill /></a>
        {!userState.isInRoom &&
          <InputForm
            classNames={`form-control form-control-sm rounded-3 ms-3 ${userState.username.length < 3 ? "is-invalid" : "is-valid"}`}
            placeholder={"Enter your username"}
            value={userState.username}
            trim={true}
            isEnabled={true}
            onChange={(value: string) => dispatch(updatedUsername(value))}
          />
        }
        {userState.isInRoom &&
          <span className="text-white ms-3">Your username: <b>{userState.username}</b></span>
        }
      </div>
      {
        userState.isInRoom &&
        <div className="justify-content-end me-3">
          <Button
            text={<><BsDoorOpenFill /> Leave room</>}
            classNames={"btn btn-danger btn-sm"}
            onClick={handleLeaveRoomButtonClick}
          />
        </div>
      }
    </nav>
  )
}
import { BsFillCameraReelsFill } from "react-icons/bs";
import { InputForm } from "./InputForm";
import { useAppSelector } from "../redux/hooks";
import { useDispatch } from "react-redux";
import { updatedUsername } from "../redux/slices/userState-slice";

export default function Header() {
  const userState = useAppSelector((state) => state.userState);
  const dispatch = useDispatch();

  return (
    <nav className="navbar navbar-dark bg-dark mb-3">
      <a className="navbar-brand mx-3" href="/"><i><b>SharedWatch</b></i> <BsFillCameraReelsFill /></a>
      <div className="d-flex align-items-center mx-3">
        {
          !userState.isInRoom &&
          <>
            <span className="text-white" style={{marginRight: "15px"}}>Your username: </span>
            <InputForm
              classNames={"rounded-3"}
              placeholder={"Type here"}
              value={userState.username}
              trim={true}
              onChange={(value: string) => dispatch(updatedUsername(value))}
            />
          </>
        }
        {
          userState.isInRoom &&
          <span className="text-white" style={{marginRight: "15px"}}><b>{userState.username}</b></span>
        }
      </div>
    </nav>
  )
}

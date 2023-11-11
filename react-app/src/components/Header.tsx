import { BsFillCameraReelsFill } from "react-icons/bs";
import { InputForm } from "./InputForm";
import { useAppSelector } from "../redux/hooks";
import { useDispatch } from "react-redux";
import { updatedUsername } from "../redux/slices/userState-slice";

export default function Header() {
  const userState = useAppSelector((state) => state.userState);
  const dispatch = useDispatch();

  return (
    <nav className="navbar navbar-dark bg-dark mb-4">
      <a className="navbar-brand mx-3" href="/"></a>
      <div className="d-flex align-items-center mx-3">
        {
          !userState.isInRoom &&
          <>

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

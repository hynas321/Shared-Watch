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
  <div className="d-flex align-items-center">
    <a className="navbar-brand ms-3" href="/"><i><b>SharedWatch</b></i> <BsFillCameraReelsFill /></a>
    {!userState.isInRoom &&
      <InputForm
        classNames={`form-control form-control-sm rounded-3 ms-3 ${userState.username.length < 3 ? "is-invalid" : "is-valid"}`}
        placeholder={"Enter your username"}
        value={userState.username}
        trim={true}
        onChange={(value: string) => dispatch(updatedUsername(value))}
      />
    }
    {userState.isInRoom &&
      <span className="text-white ms-3">Your username: <b>{userState.username}</b></span>
    }
  </div>
</nav>
  )
}
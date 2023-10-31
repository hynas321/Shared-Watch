import { BsFillCameraReelsFill } from "react-icons/bs";
import Button from "./Button";
import { useState } from "react";

export default function Header() {
  const [isLoggedIn, setIsLoggedIn] = useState<boolean>(false);
  const [username] = useState<string>("Username");

  return (
    <nav className="navbar navbar-dark bg-dark mb-3">
      <a className="navbar-brand mx-3" href="/"><i><b>SharedWatch</b></i> <BsFillCameraReelsFill /></a>
      <div className="d-flex align-items-center">
        {
          !isLoggedIn &&
          <>
            <Button
              text={"Log in"}
              classNames={"btn btn-primary"}
              styles={{}}
              onClick={() => setIsLoggedIn(true)}
            />
            <Button
              text={"Sign up"}
              classNames={"btn btn-secondary mx-3"}
              styles={{}}
              onClick={() => setIsLoggedIn(true)}
            />
          </>
        }
        {
          isLoggedIn &&
          <>
            <span className="text-white mx-3">{username}</span>
            <Button
              text={"Log out"}
              classNames={"btn btn-secondary mx-3"}
              styles={{}}
              onClick={() => setIsLoggedIn(false)}
            />
          </>
        }

      </div>
    </nav>
  )
}

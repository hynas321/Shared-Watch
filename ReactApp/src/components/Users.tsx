import { useContext } from "react";
import {
  BsFillPersonFill,
  BsFillPersonXFill,
  BsShieldFillCheck,
  BsShieldFillMinus,
  BsShieldFillPlus,
} from "react-icons/bs";
import Button from "./shared/Button";
import { AppStateContext, AppHubContext } from "../context/AppContext";
import { HubMessages } from "../classes/constants/HubMessages";

export default function Users() {
  const appState = useContext(AppStateContext);
  const appHub = useContext(AppHubContext);

  const handleAdminStatusButtonClick = async (adminStatus: boolean, username: string) => {
    await appHub.invoke(HubMessages.SetAdminStatus, appState.roomHash.value, username, adminStatus);
  };

  const handleKickOutUserButtonClick = async (event: any, username: string) => {
    await appHub.invoke(HubMessages.KickOut, appState.roomHash.value, username);

    event.preventDefault();
  };

  return (
    <ul className="list-group rounded-3">
      {appState.users.value?.length !== 0 ? (
        appState.users.value?.map((user, index) => (
          <li
            key={index}
            className="d-flex justify-content-between align-items-center border border-secondary list-group-item bg-muted border-2"
          >
            <span
              className={appState.username.value === user.username ? "text-orange" : "text-dark"}
            >
              {user.isAdmin ? <BsShieldFillCheck /> : <BsFillPersonFill />} {user.username}
            </span>
            <div>
              {user.isAdmin &&
                appState.isAdmin.value &&
                appState.username.value !== user.username && (
                  <Button
                    text={<BsShieldFillMinus />}
                    classNames="btn btn-success me-2 text-orange"
                    onClick={() => handleAdminStatusButtonClick(false, user.username)}
                  />
                )}
              {!user.isAdmin &&
                appState.isAdmin.value &&
                appState.username.value !== user.username && (
                  <Button
                    text={<BsShieldFillPlus />}
                    classNames="btn btn-success me-2"
                    onClick={() => handleAdminStatusButtonClick(true, user.username)}
                  />
                )}
              {appState.isAdmin.value && appState.username.value !== user.username && (
                <Button
                  text={<BsFillPersonXFill />}
                  classNames="btn btn-danger"
                  onClick={(event) => handleKickOutUserButtonClick(event, user.username)}
                />
              )}
            </div>
          </li>
        ))
      ) : (
        <h6 className="text-white text-center">No users to display</h6>
      )}
    </ul>
  );
}

import { useContext } from "react";
import { BsFillPersonFill, BsFillPersonXFill, BsShieldFillCheck, BsShieldFillMinus, BsShieldFillPlus } from "react-icons/bs";
import Button from "./Button";
import { AppStateContext, RoomHubContext } from "../context/RoomHubContext";
import { HubEvents } from "../classes/HubEvents";

export default function Users() {
  const appState = useContext(AppStateContext);
  const roomHub = useContext(RoomHubContext);

  const handleAdminStatusButtonClick = (adminStatus: boolean, index: number) => {
    if (appState.users.value === null) {
      return;
    }

    appState.users.value[index].isAdmin = adminStatus;
  }

  const handleRemoveUserButtonClick = (event: any, index: number) => {
    event.preventDefault();
  }

    return (
      <ul className="list-group rounded-3">
      {
        appState.users.value?.length !== 0 ? (
          appState.users.value?.map((user, index) => (
            <li 
              key={index}
              className="d-flex justify-content-between align-items-center border border-secondary list-group-item bg-muted border-2"
            >
              <span className={appState.username.value == user.username ? "text-orange" : "text-dark"}>
                { user.isAdmin ? <BsShieldFillCheck /> : <BsFillPersonFill /> } {user.username}
              </span>
              <div>
                {
                  (user.isAdmin && appState.isAdmin.value === true &&appState.username.value != user.username) &&
                    <Button
                      text={<BsShieldFillMinus />}
                      classNames="btn btn-outline-warning me-2"
                      onClick={() => handleAdminStatusButtonClick(false, index)}
                    />
                }
                {
                  (!user.isAdmin && appState.isAdmin.value === true && appState.username.value != user.username) &&
                  <Button
                    text={<BsShieldFillPlus />}
                    classNames="btn btn-outline-warning me-2"
                    onClick={() => handleAdminStatusButtonClick(true, index)}
                  />
                }
                {
                  (appState.isAdmin.value === true && appState.username.value != user.username) &&
                  <Button
                    text={<BsFillPersonXFill />}
                    classNames="btn btn-outline-danger"
                    onClick={() => handleRemoveUserButtonClick(event, index)}
                  />
                }
              </div>
          </li>
          ))
        ) :
        <h6 className="text-white text-center">No users to display</h6>
      }
      </ul>
    )
}
  
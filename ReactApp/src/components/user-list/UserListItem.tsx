import {
  BsFillPersonFill,
  BsFillPersonXFill,
  BsShieldFillCheck,
  BsShieldFillMinus,
  BsShieldFillPlus,
} from "react-icons/bs";
import Button from "../shared/Button";
import { HubMessages } from "../../classes/constants/HubMessages";
import { useContext } from "react";
import { AppHubContext } from "../../context/AppContext";

interface Props {
  user: {
    username: string;
    isAdmin: boolean;
  };
  currentUsername: string;
  isCurrentUserAdmin: boolean;
  roomHash: string;
}

export default function UserListItem({
  user,
  currentUsername,
  isCurrentUserAdmin,
  roomHash
}: Props) {
  const isMe = currentUsername === user.username;
  const appHub = useContext(AppHubContext);

  const handleAdminStatus = async (newStatus: boolean) => {
    await appHub.invoke(HubMessages.SetAdminStatus, roomHash, user.username, newStatus);
  };

  const handleKick = async (event: React.MouseEvent) => {
    event.preventDefault();
    await appHub.invoke(HubMessages.KickOut, roomHash, user.username);
  };

  return (
    <li
      className="d-flex justify-content-between align-items-center border border-secondary list-group-item bg-muted border-2"
    >
      <span className={isMe ? "text-orange" : "text-dark"}>
        {user.isAdmin ? <BsShieldFillCheck /> : <BsFillPersonFill />} {user.username}
      </span>
      <div>
        {user.isAdmin && isCurrentUserAdmin && !isMe && (
          <Button
            text={<BsShieldFillMinus />}
            classNames="btn btn-success me-2 text-orange"
            onClick={() => handleAdminStatus(false)}
          />
        )}
        {!user.isAdmin && isCurrentUserAdmin && !isMe && (
          <Button
            text={<BsShieldFillPlus />}
            classNames="btn btn-success me-2"
            onClick={() => handleAdminStatus(true)}
          />
        )}
        {isCurrentUserAdmin && !isMe && (
          <Button
            text={<BsFillPersonXFill />}
            classNames="btn btn-danger"
            onClick={handleKick}
          />
        )}
      </div>
    </li>
  );
}

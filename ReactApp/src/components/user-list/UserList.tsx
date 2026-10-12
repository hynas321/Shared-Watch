import { useContext } from "react";
import { BsPeopleFill } from "react-icons/bs";
import { AppStateContext } from "../../context/AppContext";
import UserListItem from "./UserListItem";

export default function UserList() {
  const appState = useContext(AppStateContext);

  const { username, users, roomHash, isAdmin } = appState;

  return (
    <ul className="list-group rounded-3 user-list">
      {users.value?.length ? (
        users.value.map((user, index) => (
          <UserListItem
            key={index}
            user={user}
            currentUsername={username.value}
            isCurrentUserAdmin={isAdmin.value}
            roomHash={roomHash.value}
          />
        ))
      ) : (
        <div className="empty-state text-dark text-center" role="status">
          <BsPeopleFill className="empty-state-icon" aria-hidden="true" />
          <span>No other users are here yet</span>
        </div>
      )}
    </ul>
  );
}

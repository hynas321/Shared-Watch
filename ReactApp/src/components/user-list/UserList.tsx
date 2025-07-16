import { useContext } from "react";
import { AppStateContext } from "../../context/AppContext";
import UserListItem from "./UserListItem";

export default function UserList() {
  const appState = useContext(AppStateContext);

  const { username, users, roomHash, isAdmin } = appState;

  return (
    <ul className="list-group rounded-3">
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
        <h6 className="text-white text-center">No users to display</h6>
      )}
    </ul>
  );
}

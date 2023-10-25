import { useEffect, useState } from "react";
import { User } from "../types/User";
import { BsFillPersonXFill } from "react-icons/bs";
import Button from "./Button";

export default function Users() {
  const [users, setUsers] = useState<User[]>([]);

  useEffect(() => {
    const user = {
      username: "User1"
    }

    setUsers([user]);
  }, []);

    return (
      <ul className="list-group rounded-3">
      {
        users.length !== 0 ? (
          users.map((user, index) => (
            <li key={index} className="border border-secondary list-group-item bg-muted border-2 d-flex justify-content-between align-items-center">
            <span className="text-dark">{user.username}</span>
            <Button
              text={<BsFillPersonXFill />}
              classNames="btn btn-outline-danger btn-sm"
              styles={{marginLeft: "5px"}}
              onClick={() => {}}
            />
          </li>
          ))
        ) :
        <h6 className="text-white text-center">No users to display</h6>
      }
      </ul>
    )
}
  
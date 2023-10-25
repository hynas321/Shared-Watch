import { useEffect, useState } from "react";
import { User } from "../types/User";
import { BsFillPersonFill, BsFillPersonXFill, BsShieldFillCheck, BsShieldFillMinus, BsShieldFillPlus } from "react-icons/bs";
import Button from "./Button";

export interface UsersProps {
  onChange?: (users: User[]) => void;
}

export default function Users({onChange}: UsersProps) {
  const [users, setUsers] = useState<User[]>([]);

  useEffect(() => {
    const user1 = {
      username: "User1",
      isAdmin: true
    }

    const user2 = {
      username: "User2",
      isAdmin: false
    }

    setUsers([user1, user2]);
  }, []);

  useEffect(() => {
    if (onChange) {
      onChange(users);
    }
  }, [users]);

  const handleAdminStatusButtonClick = (adminStatus: boolean, index: number) => {
    const updatedUsers = [...users];
  
    updatedUsers[index].isAdmin = adminStatus;
    setUsers(updatedUsers);
  }

  const handleRemoveUserButtonClick = (event: any, index: number) => {
    console.log(index);
    event.preventDefault();
    setUsers(users.filter((_, i) => i !== index));
  }

    return (
      <ul className="list-group rounded-3">
      {
        users.length !== 0 ? (
          users.map((user, index) => (
            <li 
              key={index}
              className="d-flex justify-content-between align-items-center border border-secondary list-group-item bg-muted border-2"
            >
              <span className="text-dark">
                { user.isAdmin ? <BsShieldFillCheck /> : <BsFillPersonFill /> } {user.username}
              </span>
              <div>
                {
                  user.isAdmin ? 
                    <Button
                    text={<BsShieldFillMinus />}
                    classNames="btn btn-outline-warning"
                    styles={{marginLeft: "5px"}}
                    onClick={() => handleAdminStatusButtonClick(false, index)}
                  />
                  :
                  <Button
                    text={<BsShieldFillPlus />}
                    classNames="btn btn-outline-warning"
                    styles={{marginLeft: "5px"}}
                    onClick={() => handleAdminStatusButtonClick(true, index)}
                  />
                }
                <Button
                  text={<BsFillPersonXFill />}
                  classNames="btn btn-outline-danger"
                  styles={{marginLeft: "5px"}}
                  onClick={() => handleRemoveUserButtonClick(event, index)}
                />
              </div>
          </li>
          ))
        ) :
        <h6 className="text-white text-center">No users to display</h6>
      }
      </ul>
    )
}
  
import { User } from "../types/User";

export interface Users {
  users: User[];
}

export default function Users({users}: Users) {
  return (
    <ul className="list-group rounded-3">
    {
      users.length !== 0 ? (
        users.map((user, index) => (
          <li 
            key={index}
            className="border border-secondary list-group-item bg-muted border-2 opacity-75"
          >
            <h6 className="text-dark">{user.username}</h6>
          </li>
        ))
      ) :
      <h6 className="text-white text-center">No users to display</h6>
    }
    </ul>
  )
}
  
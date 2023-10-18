import { Room } from "../types/Room";

export interface RoomListProps {
  title?: string,
  list: Room[];
}

export default function RoomList({ title, list }: RoomListProps) {
  return (
    <>
      { 
        title &&
        <h3>{title}</h3>
      }
      <ul className="list-group">
        {
          list.map((item, index) => (
            <li key={index} className="list-group-item">
              {item.id}
            </li>
          ))
        }
      </ul>
    </>
  );
}
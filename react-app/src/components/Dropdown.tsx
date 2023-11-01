import { ReactNode } from "react";

export interface DropdownProps {
  title: string;
  body: ReactNode;
}

export default function Dropdown({title, body}: DropdownProps) {
  return (
    <div style={{position: "relative"}}>
      <button
        className="btn btn-primary dropdown-toggle"
        type="button"
        data-bs-toggle="dropdown"
        aria-haspopup="true"
        aria-expanded="false">
          {title}
      </button>
      <div className="dropdown-menu">
        {body}
      </div>
    </div>
  )
}

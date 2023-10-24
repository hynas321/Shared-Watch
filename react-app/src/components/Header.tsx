import { BsFillCameraReelsFill } from "react-icons/bs";
import Button from "./Button";

export interface HeaderProps {
  title?: string;
}

export default function Header({title}: HeaderProps) {
  return (
    <nav className="navbar navbar-dark bg-dark mb-3">
      <a className="navbar-brand mx-3" href="/"><i><b>SharedWatch</b></i> <BsFillCameraReelsFill /></a>
      { 
        title && <h4 className="text-white mx-auto">{title}</h4>
      }
      <div>
        <Button
          text={"Log in"}
          bootstrapClass={"btn-secondary"}
          styles={{}}
          onClick={() => {}}
        />
        <Button
          text={"Sign up"}
          bootstrapClass={"btn-secondary mx-3"}
          styles={{}}
          onClick={() => {}}
        />
      </div>
    </nav>
  )
}

import { useState } from "react";
import { SidebarElement } from "../types/SidebarElement"
import RoomInfo from "./RoomInfo";
import Button from "./Button";

export default function SidebarMenu() {
  const styles = {
    cursor: "pointer"
  };

  const element: SidebarElement = {
    image: undefined,
    text: "Test"
  };

  const elements: SidebarElement[] = [
    element,
    element,
    element,
    element,
    element
  ];
  
  return (
    <>
      <div className="row">
        <div className="btn-group" role="group">
          <Button 
              text="Playlist"
              bootstrapClass="btn-primary"
              onClick={() => { }} 
              styles={{}}
          />
          <Button 
              text="Chat"
              bootstrapClass="btn-secondary"
              onClick={() => { }} 
              styles={{}}
          />
          <Button 
              text="Users"
              bootstrapClass="btn-secondary"
              onClick={() => { }} 
              styles={{}}
          />
          <Button 
              text="Settings"
              bootstrapClass="btn-secondary"
              onClick={() => { }} 
              styles={{}}
          />
        </div>
      </div>
      <ul className="list-group rounded-bottom-5">
        {
          elements.map((item, index) => (
            <li 
              key={index}
              className="border border-secondary list-group-item bg-muted border-2 opacity-75"
              style={styles}
            >
              <h5 className="d-inline">{item.image}</h5>
              <h6 className="text-dark">{item.text}</h6>
            </li>
          ))
        }
      </ul>
    </>
  )
}
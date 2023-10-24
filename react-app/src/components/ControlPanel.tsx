import { useEffect, useState } from "react";
import { PanelsEnum } from "../enums/PanelsEnum";
import Button from "./Button";
import { BsGearFill } from 'react-icons/bs';
import Chat from "./Chat";
import Playlist from "./Playlist";
import Users from "./Users";
import Settings from "./Settings";

export default function ControlPanel() {
  const [activePanel, setActivePanel] = useState<PanelsEnum>(PanelsEnum.Chat);

  const element: any = {
    image: undefined,
    text: "User1: Test"
  };

  const elements: any[] = [
    element,
    element,
    element,
    element,
    element
  ];

  useEffect(() => {}, [activePanel])

  const handlePanelButtonClick = (panelsEnumValue: PanelsEnum) => {
    setActivePanel(panelsEnumValue);
  }

  return (
    <div>
      <div className="rounded-top-5 bg-dark pt-3 pb-3 px-4">
        <h4 className="text-white text-center">Test room</h4>
      </div>
      <div className="row">
        <div className="btn-group" role="group">
          <Button 
            text={<><span className="badge rounded-pill bg-danger mt-2">5</span> Chat</>}
            bootstrapClass={activePanel === PanelsEnum.Chat ? "btn-primary" : "btn-secondary"}
            onClick={() => handlePanelButtonClick(PanelsEnum.Chat)} 
          />
          <Button 
            text={<><span className="badge rounded-pill bg-success mt-2">10</span> Playlist</>}
            bootstrapClass={activePanel === PanelsEnum.Playlist ? "btn-primary" : "btn-secondary"}
            onClick={() => handlePanelButtonClick(PanelsEnum.Playlist)} 
          />
          <Button 
            text={<><span className="badge rounded-pill bg-success mt-2">5/10</span> Users</>}
            bootstrapClass={activePanel === PanelsEnum.Users ? "btn-primary" : "btn-secondary"}
            onClick={() => handlePanelButtonClick(PanelsEnum.Users)} 
          />
          <Button 
            text={<><BsGearFill /> Settings</>}
            bootstrapClass={activePanel === PanelsEnum.Settings ? "btn-primary" : "btn-secondary"}
            onClick={() => handlePanelButtonClick(PanelsEnum.Settings)} 
          />
        </div>
      </div>
      <div className="rounded-bottom-5 bg-dark pt-4 pb-4 px-4">
        { activePanel === PanelsEnum.Chat && <Chat chatMessages={elements} /> }
        { activePanel === PanelsEnum.Playlist && <Playlist queuedVideos={[]} /> }
        { activePanel === PanelsEnum.Users && <Users users={[]} /> }
        { activePanel === PanelsEnum.Settings && <Settings /> }
      </div>
    </div>
  )
}
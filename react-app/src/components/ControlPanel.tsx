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

  useEffect(() => {}, [activePanel])

  const handlePanelButtonClick = (panelsEnumValue: PanelsEnum) => {
    setActivePanel(panelsEnumValue);
  }

  return (
    <div>
      <div className="rounded-top-5 bg-dark pt-3 pb-3 px-4">
        <h5 className="text-white text-center">Test room</h5>
      </div>
      <div className="row">
        <div className="btn-group" role="group">
          <Button 
            text={<><span className="badge rounded-pill bg-danger mt-2">5</span> Chat</>}
            classNames={activePanel === PanelsEnum.Chat ? "btn btn-primary btn-rectangular" : "btn btn-secondary btn-rectangular"}
            onClick={() => handlePanelButtonClick(PanelsEnum.Chat)} 
          />
          <Button 
            text={<><span className="badge rounded-pill bg-success mt-2">10</span> Playlist</>}
            classNames={activePanel === PanelsEnum.Playlist ? "btn btn-primary btn-rectangular" : "btn btn-secondary btn-rectangular"}
            onClick={() => handlePanelButtonClick(PanelsEnum.Playlist)} 
          />
          <Button 
            text={<><span className="badge rounded-pill bg-success mt-2">5/10</span> Users</>}
            classNames={activePanel === PanelsEnum.Users ? "btn btn-primary btn-rectangular" : "btn btn-secondary btn-rectangular"}
            onClick={() => handlePanelButtonClick(PanelsEnum.Users)} 
          />
          <Button 
            text={<><BsGearFill /> Settings</>}
            classNames={activePanel === PanelsEnum.Settings ? "btn btn-primary btn-rectangular" : "btn btn-secondary btn-rectangular"}
            onClick={() => handlePanelButtonClick(PanelsEnum.Settings)} 
          />
        </div>
      </div>
      <div className="rounded-bottom-5 bg-dark pt-4 pb-4 px-4">
        { activePanel === PanelsEnum.Chat && <Chat /> }
        { activePanel === PanelsEnum.Playlist && <Playlist /> }
        { activePanel === PanelsEnum.Users && <Users /> }
        { activePanel === PanelsEnum.Settings && <Settings /> }
      </div>
    </div>
  )
}
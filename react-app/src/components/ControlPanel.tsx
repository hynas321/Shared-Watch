import { useEffect, useState } from "react";
import { PanelsEnum } from "../enums/PanelsEnum";
import Button from "./Button";
import { BsDoorOpenFill, BsFillChatTextFill, BsGearFill } from 'react-icons/bs';
import Chat from "./Chat";
import Playlist from "./Playlist";
import Users from "./Users";
import Settings from "./Settings";
import { ChatMessage } from "../types/ChatMessage";
import { QueuedVideo } from "../types/QueuedVideo";
import { User } from "../types/User";
import { useNavigate } from "react-router-dom";
import { ClientEndpoints } from "../classes/ClientEndpoints";

export default function ControlPanel() {
  const [activePanel, setActivePanel] = useState<PanelsEnum>(PanelsEnum.Chat);
  const [unreadChatMessagesCount, setUnreadChatMessagesCount] = useState<number>(0);
  const [queuedVideosCount, setQueuedVideosCount] = useState<number>(0);
  const [usersCount, setUsersCount] = useState<number>(0);
  const [maxUsersCount] = useState<number>(10);
  const navigate = useNavigate();

  useEffect(() => {
    //fetch number values
  }, []);

  const handlePanelButtonClick = (panelsEnumValue: PanelsEnum) => {
    setActivePanel(panelsEnumValue);
  }

  const handleChatChange = (chatMessages: ChatMessage[]) => {
    setUnreadChatMessagesCount(chatMessages.length);
  }

  const handlePlaylistChange = (queuedVideos: QueuedVideo[]) => {
    setQueuedVideosCount(queuedVideos.length);
  }

  const handleUsers = (users: User[]) => {
    setUsersCount(users.length);
  }

  const handleLeaveRoomButtonClick = () => {
    navigate(ClientEndpoints.mainMenu); 
  }

  return (
    <div>
      <div className="rounded-top-5 bg-dark pt-3 pb-3 px-4">
      <div className="d-flex align-items-center">
        <div className="text-center flex-grow-1">
          <h5 className="text-white">Test room</h5>
        </div>
      </div>
      <div className="d-flex justify-content-center">
        <Button
          text={<><BsDoorOpenFill /> Leave</>}
          classNames={"btn btn-danger btn-sm"}
          onClick={handleLeaveRoomButtonClick}
        />
      </div>
      </div>
      <div className="row">
        <div className="btn-group" role="group">
          <Button 
            text={
              unreadChatMessagesCount !== 0 ? (
                <><span className="badge rounded-pill bg-danger mt-2">{unreadChatMessagesCount}</span> Chat</>
              ) : (
                <><BsFillChatTextFill /> Chat</>
              )
            }
            classNames={activePanel === PanelsEnum.Chat ? "btn btn-primary btn-rectangular" : "btn btn-secondary btn-rectangular"}
            onClick={() => handlePanelButtonClick(PanelsEnum.Chat)} 
          />
          <Button 
            text={
              <>
                <span className="badge rounded-pill bg-success mt-2">{queuedVideosCount}</span> Playlist
              </>
            }
            classNames={activePanel === PanelsEnum.Playlist ? "btn btn-primary btn-rectangular" : "btn btn-secondary btn-rectangular"}
            onClick={() => handlePanelButtonClick(PanelsEnum.Playlist)} 
          />
          <Button 
            text={
              <>
                <span className="badge rounded-pill bg-success mt-2">{usersCount}/{maxUsersCount}</span> Users
              </>
            }
            classNames={activePanel === PanelsEnum.Users ? "btn btn-primary btn-rectangular" : "btn btn-secondary btn-rectangular"}
            onClick={() => handlePanelButtonClick(PanelsEnum.Users)} 
          />
          <Button 
            text={
              <><BsGearFill /> Settings</>
            }
            classNames={activePanel === PanelsEnum.Settings ? "btn btn-primary btn-rectangular" : "btn btn-secondary btn-rectangular"}
            onClick={() => handlePanelButtonClick(PanelsEnum.Settings)} 
          />
        </div>
      </div>
      <div className="rounded-bottom-5 bg-dark pt-4 pb-4 px-4">
        { activePanel === PanelsEnum.Chat && <Chat onChange={handleChatChange} /> }
        { activePanel === PanelsEnum.Playlist && <Playlist onChange={handlePlaylistChange} /> }
        { activePanel === PanelsEnum.Users && <Users onChange={handleUsers} /> }
        { activePanel === PanelsEnum.Settings && <Settings /> }
      </div>
    </div>
  )
}
import { useContext, useEffect } from "react";
import { PanelsEnum } from "../enums/PanelsEnum";
import Button from "./Button";
import { BsFillChatTextFill, BsFillLockFill, BsGearFill } from 'react-icons/bs';
import Chat from "./Chat";
import Playlist from "./Playlist";
import Users from "./Users";
import Settings from "./Settings";
import { RoomTypesEnum } from "../enums/RoomTypesEnum";
import { RoomHubContext, appState } from "../context/RoomHubContext";
import * as signalR from "@microsoft/signalr";
import { HubEvents } from "../classes/HubEvents";
import { QueuedVideo } from "../types/QueuedVideo";
import { User } from "../types/User";
import { toast } from "react-toastify";
import { ClientEndpoints } from "../classes/ClientEndpoints";
import { useNavigate } from "react-router-dom";
import { HttpManager } from "../classes/HttpManager";

export default function ControlPanel() {
  const roomHub = useContext(RoomHubContext);
  const navigate = useNavigate();

  const httpManager = new HttpManager();

  const handlePanelButtonClick = (panelsEnumValue: PanelsEnum) => {
    appState.activePanel.value = panelsEnumValue;
  }

  useEffect(() => {
    if (roomHub.getState() !== signalR.HubConnectionState.Connected) {
      return;
    }

    roomHub.on(HubEvents.OnAddChatMessage, (chatMessageSerialized: string) => {
      const chatMessage = JSON.parse(chatMessageSerialized);

      appState.chatMessages.value = [...appState.chatMessages.value, chatMessage];

      if (appState.activePanel.value !== PanelsEnum.Chat) {
        appState.unreadChatMessagesCount.value = appState.unreadChatMessagesCount.value + 1;
      }
    });

    return () => {
      roomHub.off(HubEvents.OnAddChatMessage);
    }
  }, [roomHub.getState()]);

  useEffect(() => {
    if (roomHub.getState() !== signalR.HubConnectionState.Connected) {
      return;
    }

    roomHub.on(HubEvents.OnAddQueuedVideo, (queuedVideoSerialized: string) => {
      const queuedVideo: QueuedVideo = JSON.parse(queuedVideoSerialized);

      appState.queuedVideos.value = [...appState.queuedVideos.value, queuedVideo];
    });

    roomHub.on(HubEvents.OnDeleteQueuedVideo, (removedIndex: number) => {

      appState.queuedVideos.value = appState.queuedVideos.value.filter(
        (_, index) => index !== removedIndex
      );
    });

    return () => {
      roomHub.off(HubEvents.OnAddQueuedVideo);
      roomHub.off(HubEvents.OnDeleteQueuedVideo);
    }
  }, [roomHub.getState()]);

  useEffect(() => {
    if (roomHub.getState() !== signalR.HubConnectionState.Connected) {
      return;
    }
    
    roomHub.on(HubEvents.OnJoinRoom, (newUser: User) => {
      appState.users.value = [...appState.users.value, newUser];
    });

    roomHub.on(HubEvents.OnLeaveRoom, (removedUser: User) => {
      appState.users.value = appState.users.value.filter(
        (user) => user.username !== removedUser.username
      );
    });

    return () => {
      roomHub.off(HubEvents.OnJoinRoom);
      roomHub.off(HubEvents.OnLeaveRoom);
    }
  }, [roomHub.getState()]);

  useEffect(() => {
    if (roomHub.getState() !== signalR.HubConnectionState.Connected) {
      return;
    }

    roomHub.on(HubEvents.OnKickOut, (removedUserSerialized: string) => {
      const removedUser: User = JSON.parse(removedUserSerialized);
      const isCurrentUser = removedUser.username === appState.username.value;

      if (isCurrentUser) {
        toast.error("You have been kicked out");
        httpManager.leaveRoom(appState.roomHash.value);
        navigate(`${ClientEndpoints.mainMenu}`, { replace: true });
        return;
      }

      appState.users.value = appState.users.value.filter(
        (user) => user.username !== removedUser.username
      );
    });

    return () => {
      roomHub.off(HubEvents.OnKickOut);
    }
  }, [roomHub.getState()]);
  
  return (
    <div>
      <div className="rounded-top-5 bg-dark bg-opacity-50 pt-3 pb-3 px-4">
      <div className="d-flex align-items-center">
        <div className="text-center flex-grow-1">
          <h5 className="text-white">
          {appState.roomType.value === RoomTypesEnum.private && <BsFillLockFill />}{appState.roomName.value}
          </h5> 
        </div>
      </div>
      </div>
      <div className="row">
        <div className="btn-group" role="group">
          <Button 
            text={
              appState.unreadChatMessagesCount.value !== 0 ? (
                <><span className="badge rounded-pill bg-danger mt-2">{appState.unreadChatMessagesCount.value}</span> Chat</>
              ) : (
                <><BsFillChatTextFill /> Chat</>
              )
            }
            classNames={appState.activePanel.value === PanelsEnum.Chat ? "btn btn-primary btn-rectangular" : "btn btn-secondary btn-rectangular"}
            onClick={() => handlePanelButtonClick(PanelsEnum.Chat)} 
          />
          <Button 
            text={<><span className="badge rounded-pill bg-success mt-2">{appState.queuedVideos.value?.length}</span> Playlist</>}
            classNames={appState.activePanel.value === PanelsEnum.Playlist ? "btn btn-primary btn-rectangular" : "btn btn-secondary btn-rectangular"}
            onClick={() => handlePanelButtonClick(PanelsEnum.Playlist)} 
          />
          <Button 
            text={<><span className="badge rounded-pill bg-success mt-2">{appState.users.value?.length}/{appState.roomSettings.value?.maxUsers}</span> Users</>}
            classNames={appState.activePanel.value === PanelsEnum.Users ? "btn btn-primary btn-rectangular" : "btn btn-secondary btn-rectangular"}
            onClick={() => handlePanelButtonClick(PanelsEnum.Users)} 
          />
          <Button 
            text={
              <><BsGearFill /> Settings</>
            }
            classNames={appState.activePanel.value === PanelsEnum.Settings ? "btn btn-primary btn-rectangular" : "btn btn-secondary btn-rectangular"}
            onClick={() => handlePanelButtonClick(PanelsEnum.Settings)} 
          />
        </div>
      </div>
      <div className="rounded-bottom-5 bg-dark bg-opacity-50 pt-4 pb-4 px-4">
        { appState.activePanel.value === PanelsEnum.Chat && <Chat /> }
        { appState.activePanel.value === PanelsEnum.Playlist && <Playlist /> }
        { appState.activePanel.value === PanelsEnum.Users && <Users  /> }
        { appState.activePanel.value === PanelsEnum.Settings && <Settings /> }
      </div>
    </div>
  )
}
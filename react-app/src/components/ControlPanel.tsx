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
import { PlaylistVideo } from "../types/PlaylistVideo";
import { User } from "../types/User";
import { toast } from "react-toastify";
import { ClientEndpoints } from "../classes/ClientEndpoints";
import { useNavigate } from "react-router-dom";
import { HttpManager } from "../classes/HttpManager";
import { UserPermissions } from "../types/UserPermissions";
import { ToastNotificationEnum } from "../enums/ToastNotificationEnum";

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

    roomHub.on(HubEvents.OnAddPlaylistVideo, (playlistVideoSerialized: string | null) => {
      if (playlistVideoSerialized == null) {
        return;
      }

      const playlistVideo: PlaylistVideo = JSON.parse(playlistVideoSerialized);

      appState.playlistVideos.value = [...appState.playlistVideos.value, playlistVideo];
    });

    roomHub.on(HubEvents.OnDeletePlaylistVideo, (playlistVideoHash: string) => {
      appState.playlistVideos.value = appState.playlistVideos.value.filter(
        (video) => video.hash !== playlistVideoHash
      );
    });

    roomHub.on(HubEvents.OnJoinRoom, (newUser: User) => {
      appState.users.value = [...appState.users.value, newUser];
    });

    roomHub.on(HubEvents.OnLeaveRoom, (removedUser: User) => {
      appState.users.value = appState.users.value.filter(
        (user) => user.username !== removedUser.username
      );
    });

    roomHub.on(HubEvents.OnKickOut, (removedUserSerialized: string) => {
      const removedUser: User = JSON.parse(removedUserSerialized);
      const isCurrentUser = removedUser.username === appState.username.value;
      
      if (isCurrentUser) {
        toast.error(
          "You have been kicked out", {
            containerId: ToastNotificationEnum.Main,
          }
        );
        httpManager.leaveRoom(appState.roomHash.value);
        navigate(`${ClientEndpoints.mainMenu}`, { replace: true });
        return;
      }

      toast.info(
        `User ${removedUser.username} has been kicked out`, {
          containerId: ToastNotificationEnum.Room
        }
      );

      appState.users.value = appState.users.value.filter(
        (user) => user.username !== removedUser.username
      );
    });

    roomHub.on(HubEvents.OnSetAdminStatus, (updatedUserSerialized: string) => {
      const updatedUser: User = JSON.parse(updatedUserSerialized);
      const isCurrentUser = updatedUser.username === appState.username.value;

      if (isCurrentUser && updatedUser.isAdmin === true) {
        appState.isAdmin.value = updatedUser.isAdmin;
        
        toast.clearWaitingQueue({
            containerId: ToastNotificationEnum.Room
          }
        );

        toast.success(
          `You have become an admin`, {
            containerId: ToastNotificationEnum.Room
          }
        );
      } 
      else if (isCurrentUser && updatedUser.isAdmin === false) {
        appState.isAdmin.value = updatedUser.isAdmin;

        toast.clearWaitingQueue({
            containerId: ToastNotificationEnum.Room
          }
        );

        toast.error(
          `You are no longer an admin`, {
            containerId: ToastNotificationEnum.Room
          }
        );
      }
      else if (!isCurrentUser && updatedUser.isAdmin === true) {
        toast.clearWaitingQueue({
            containerId: ToastNotificationEnum.Room
          }
        );

        toast.info(
          `User ${updatedUser.username} has become an admin`, {
            containerId: ToastNotificationEnum.Room
          }
        );
      }
      else if (!isCurrentUser && updatedUser.isAdmin === false) {
        toast.clearWaitingQueue({
            containerId: ToastNotificationEnum.Room
          }
        );

        toast.info(
          `User ${updatedUser.username} is no longer an admin`, {
            containerId: ToastNotificationEnum.Room
          }
        );
      }
    
      const userIndex = appState.users.value.findIndex(
        (user) => user.username === updatedUser.username
      );
    
      if (userIndex !== -1) {
        const updatedUsers = [...appState.users.value];
        updatedUsers[userIndex] = { ...updatedUser };
    
        appState.users.value = updatedUsers;
      }
    });

    roomHub.on(HubEvents.OnSetUserPermissions, (userPermissionsSerialized: string) => {
      const userPermissions: UserPermissions = JSON.parse(userPermissionsSerialized);

      if (appState.userPermissions.value != null) {
        appState.userPermissions.value = userPermissions;
      }
    });

    roomHub.on(HubEvents.OnSetRoomPassword, (roomPassword: string, roomType: RoomTypesEnum) => {
      appState.roomPassword.value = roomPassword;
      appState.roomType.value = roomType;
    });


    return () => {
      roomHub.off(HubEvents.OnAddChatMessage);
      roomHub.off(HubEvents.OnAddPlaylistVideo);
      roomHub.off(HubEvents.OnDeletePlaylistVideo);
      roomHub.off(HubEvents.OnJoinRoom);
      roomHub.off(HubEvents.OnLeaveRoom);
      roomHub.off(HubEvents.OnKickOut);
      roomHub.off(HubEvents.OnSetAdminStatus);
      roomHub.off(HubEvents.OnSetUserPermissions);
      roomHub.off(HubEvents.OnSetRoomPassword);
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
            text={<><span className="badge rounded-pill bg-success mt-2" style={{fontSize: "11px"}}>{appState.playlistVideos.value?.length}/{appState.maxPlaylistVideos.value}</span> Playlist</>}
            classNames={appState.activePanel.value === PanelsEnum.Playlist ? "btn btn-primary btn-rectangular" : "btn btn-secondary btn-rectangular"}
            onClick={() => handlePanelButtonClick(PanelsEnum.Playlist)} 
          />
          <Button 
            text={<><span className="badge rounded-pill bg-success mt-2" style={{fontSize: "11px"}}>{appState.users.value?.length}/{appState.maxUsers.value}</span> Users</>}
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
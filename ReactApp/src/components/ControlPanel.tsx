import { useContext, useEffect } from "react";
import { PanelsEnum } from "../enums/PanelsEnum";
import { BsFillChatTextFill, BsFillLockFill, BsGearFill } from "react-icons/bs";
import Chat from "./chat/Chat";
import Playlist from "./playlist/Playlist";
import UserList from "./user-list/UserList";
import Settings from "./Settings";
import { RoomTypesEnum } from "../enums/RoomTypesEnum";
import { AppHubContext, appState } from "../context/AppContext";
import { HubMessages as HubMessages } from "../classes/constants/HubMessages";
import { PlaylistVideo } from "../types/PlaylistVideo";
import { User } from "../types/User";
import { toast } from "react-toastify";
import { ClientEndpoints } from "../classes/constants/ClientEndpoints";
import { useNavigate } from "react-router-dom";
import { UserPermissions } from "../types/UserPermissions";
import { ToastNotificationEnum } from "../enums/ToastNotificationEnum";
import Button from "./shared/Button";
import api from "../classes/Http/Api";
import { ChatMessage } from "../types/ChatMessage";

const isSamePlaylistVideo = (left: PlaylistVideo, right: PlaylistVideo): boolean =>
  left.hash != null && right.hash != null
    ? left.hash === right.hash
    : left.url === right.url;

export default function ControlPanel() {
  const navigate = useNavigate();
  const appHub = useContext(AppHubContext);

  const handlePanelButtonClick = (panelsEnumValue: PanelsEnum) => {
    appState.activePanel.value = panelsEnumValue;
  };

  useEffect(() => {
    const registeredHandlers: Array<{
      name: string;
      handler: (...args: never[]) => void;
    }> = [];
    const registerHandler = (name: string, handler: (...args: never[]) => void) => {
      registeredHandlers.push({ name, handler });
      appHub.on(name, handler);
    };

    registerHandler(HubMessages.OnAddChatMessage, (chatMessage: ChatMessage) => {
      appState.chatMessages.value = [...appState.chatMessages.value, chatMessage];

      if (appState.activePanel.value !== PanelsEnum.Chat) {
        appState.unreadChatMessagesCount.value = appState.unreadChatMessagesCount.value + 1;
      }
    });

    registerHandler(HubMessages.OnAddPlaylistVideo, (playlistVideo: PlaylistVideo | null) => {
      if (playlistVideo == null) {
        return;
      }

      if (!appState.playlistVideos.value.some((video) => isSamePlaylistVideo(video, playlistVideo))) {
        appState.playlistVideos.value = [...appState.playlistVideos.value, playlistVideo];
      }
    });

    registerHandler(HubMessages.OnDeletePlaylistVideo, (playlistVideoHash: string) => {
      appState.playlistVideos.value = appState.playlistVideos.value.filter(
        (video) => video.hash !== playlistVideoHash
      );
    });

    registerHandler(HubMessages.OnPlaylistReordered, (playlistVideos: PlaylistVideo[]) => {
      appState.playlistVideos.value = playlistVideos;
    });

    registerHandler(HubMessages.OnJoinRoom, (newUser: User) => {
      appState.users.value = [...appState.users.value, newUser];
      window.dispatchEvent(new Event("shared-watch:room-changed"));
    });

    registerHandler(HubMessages.OnLeaveRoom, (removedUser: User) => {
      appState.users.value = appState.users.value.filter(
        (user) => user.username !== removedUser.username
      );
      window.dispatchEvent(new Event("shared-watch:room-changed"));
    });

    registerHandler(HubMessages.OnKickOut, async (removedUser: User) => {
      const isCurrentUser = removedUser.username === appState.username.value;

      if (isCurrentUser) {
        toast.warning("IMPORTANT: You have been kicked out by the admin", {
          containerId: ToastNotificationEnum.Room,
        });

        await new Promise((resolve) => setTimeout(resolve, 2500));
        await api.leaveRoom(appState.roomHash.value);
        navigate(`${ClientEndpoints.mainMenu}`, { replace: true });
        return;
      }

      toast.info(`User ${removedUser.username} has been kicked out`, {
        containerId: ToastNotificationEnum.Room,
      });

      appState.users.value = appState.users.value.filter(
        (user) => user.username !== removedUser.username
      );
    });

    registerHandler(HubMessages.OnSetAdminStatus, (updatedUser: User) => {
      const isCurrentUser = updatedUser.username === appState.username.value;

      if (isCurrentUser && updatedUser.isAdmin === true) {
        appState.isAdmin.value = updatedUser.isAdmin;

        toast.clearWaitingQueue({
          containerId: ToastNotificationEnum.Room,
        });

        toast.success(`You have become an admin`, {
          containerId: ToastNotificationEnum.Room,
        });
      } else if (isCurrentUser && updatedUser.isAdmin === false) {
        appState.isAdmin.value = updatedUser.isAdmin;

        toast.clearWaitingQueue({
          containerId: ToastNotificationEnum.Room,
        });

        toast.error(`You are no longer an admin`, {
          containerId: ToastNotificationEnum.Room,
        });
      } else if (!isCurrentUser && updatedUser.isAdmin === true) {
        toast.clearWaitingQueue({
          containerId: ToastNotificationEnum.Room,
        });

        toast.info(`User ${updatedUser.username} has become an admin`, {
          containerId: ToastNotificationEnum.Room,
        });
      } else if (!isCurrentUser && updatedUser.isAdmin === false) {
        toast.clearWaitingQueue({
          containerId: ToastNotificationEnum.Room,
        });

        toast.info(`User ${updatedUser.username} is no longer an admin`, {
          containerId: ToastNotificationEnum.Room,
        });
      }

      const userIndex = appState.users.value.findIndex(
        (user) => user.username === updatedUser.username
      );

      if (userIndex !== -1) {
        const updatedUsers = [...appState.users.value];
        updatedUsers[userIndex] = {
          username: updatedUser.username,
          isAdmin: updatedUser.isAdmin === true,
        };

        appState.users.value = updatedUsers;
      } else {
        appState.users.value = [
          ...appState.users.value,
          { username: updatedUser.username, isAdmin: updatedUser.isAdmin === true },
        ];
      }
    });

    registerHandler(HubMessages.OnSetUserPermissions, (userPermissions: UserPermissions) => {
      if (appState.userPermissions.value != null) {
        appState.userPermissions.value = userPermissions;
      }
    });

    registerHandler(HubMessages.OnSetRoomPassword, (roomType: RoomTypesEnum) => {
      appState.roomType.value = roomType;
      window.dispatchEvent(new Event("shared-watch:room-changed"));
    });

    return () => {
      registeredHandlers.forEach(({ name, handler }) => appHub.off(name, handler));
    };
  }, [appHub, navigate]);

  return (
    <div className="room-control-panel">
      <div className="room-control-panel-header bg-dark bg-opacity-50 pt-3 pb-3 px-4">
        <div className="d-flex align-items-center">
          <div className="text-center flex-grow-1">
            <h5 className="room-name text-white mb-0" aria-live="polite">
              {appState.roomType.value === RoomTypesEnum.private && <BsFillLockFill />}{" "}
              {appState.roomName.value || "Room"}
            </h5>
          </div>
        </div>
      </div>
      <div className="room-panel-tabs row">
        <div className="btn-group w-100" role="tablist" aria-label="Room panels">
          <Button
            text={
              appState.unreadChatMessagesCount.value !== 0 ? (
                <>
                  <span className="badge rounded-pill bg-danger mt-2">
                    {appState.unreadChatMessagesCount.value}
                  </span>{" "}
                  Chat
                </>
              ) : (
                <>
                  <BsFillChatTextFill /> Chat
                </>
              )
            }
            classNames={
              appState.activePanel.value === PanelsEnum.Chat
                ? "btn btn-primary btn-rectangular"
                : "btn btn-secondary btn-rectangular"
            }
            onClick={() => handlePanelButtonClick(PanelsEnum.Chat)}
          />
          <Button
            text={
              <>
                <span className="badge rounded-pill bg-success mt-2" style={{ fontSize: "11px" }}>
                  {appState.playlistVideos.value?.length}/{appState.maxPlaylistVideos.value}
                </span>{" "}
                Playlist
              </>
            }
            classNames={
              appState.activePanel.value === PanelsEnum.Playlist
                ? "btn btn-primary btn-rectangular"
                : "btn btn-secondary btn-rectangular"
            }
            onClick={() => handlePanelButtonClick(PanelsEnum.Playlist)}
          />
          <Button
            text={
              <>
                <span className="badge rounded-pill bg-success mt-2" style={{ fontSize: "11px" }}>
                  {appState.users.value?.length}/{appState.maxUsers.value}
                </span>{" "}
                Users
              </>
            }
            classNames={
              appState.activePanel.value === PanelsEnum.Users
                ? "btn btn-primary btn-rectangular"
                : "btn btn-secondary btn-rectangular"
            }
            onClick={() => handlePanelButtonClick(PanelsEnum.Users)}
          />
          <Button
            text={
              <>
                <BsGearFill /> Settings
              </>
            }
            classNames={
              appState.activePanel.value === PanelsEnum.Settings
                ? "btn btn-primary btn-rectangular"
                : "btn btn-secondary btn-rectangular"
            }
            onClick={() => handlePanelButtonClick(PanelsEnum.Settings)}
          />
        </div>
      </div>
      <div className="room-control-panel-content bg-dark bg-opacity-50 pt-4 pb-4 px-4">
        {appState.activePanel.value === PanelsEnum.Chat && <Chat />}
        {appState.activePanel.value === PanelsEnum.Playlist && <Playlist />}
        {appState.activePanel.value === PanelsEnum.Users && <UserList />}
        {appState.activePanel.value === PanelsEnum.Settings && <Settings />}
      </div>
    </div>
  );
}

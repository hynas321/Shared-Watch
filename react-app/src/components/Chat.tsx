import { useContext, useEffect, useRef, useState } from "react";
import Button from "./Button";
import { ChatMessage } from "../types/ChatMessage";
import { InputField } from "./InputField";
import { BsSendFill } from "react-icons/bs";
import { AppStateContext, AppHubContext } from "../context/AppContext";
import { HubEvents } from "../classes/HubEvents";
import { LocalStorageManager } from "../classes/LocalStorageManager";

export default function Chat() {
  const appState = useContext(AppStateContext);
  const appHub = useContext(AppHubContext);

  const messagesRef = useRef<HTMLDivElement>(null);
  const [currentChatMessageText, setCurrentChatMessageText] = useState<string>("");

  const localStorageManager = new LocalStorageManager();

  useEffect(() => {
    if (appState.unreadChatMessagesCount.value !== 0) {
      appState.unreadChatMessagesCount.value = 0;
    }
  }, []);

  useEffect(() => {
    if (messagesRef.current) {
      messagesRef.current.scrollTop = messagesRef.current.scrollHeight;
    }
  }, [appState.chatMessages.value]);

  const handleTextInputChange = (text: string) => {
    setCurrentChatMessageText(text);
  }

  const handleSendMessage = () => {
    if (!currentChatMessageText || currentChatMessageText?.length === 0) {
      return;
    }

    const date = new Date();
    const currentDate: string = `${date.getHours()}:${date.getMinutes()}:${date.getSeconds()}`

    const newChatMessage: ChatMessage = {
      username: appState.username.value,
      text: currentChatMessageText,
      date: currentDate
    }

    appHub.invoke(HubEvents.AddChatMessage, appState.roomHash.value, localStorageManager.getAuthorizationToken(), newChatMessage);

    setCurrentChatMessageText("");
  };

  const handleEnterPress = (key: string) => {
    if (key === "Enter") {
      handleSendMessage();
    }
  }

  return (
    <>
      {(appState.userPermissions.value?.canAddChatMessage || appState.isAdmin.value) && (
        <div className="d-flex mb-3">
          <InputField
            classNames="form-control rounded-0"
            value={currentChatMessageText}
            trim={false}
            placeholder="Enter your message"
            isEnabled={true}
            maxCharacters={200}
            onChange={handleTextInputChange}
            onKeyDown={handleEnterPress}
          />
          <Button
            text={<BsSendFill />}
            classNames="btn btn-primary rounded-0"
            onClick={handleSendMessage}
          />
        </div>
      )}
      <div className="list-group rounded-3 control-panel-list" ref={messagesRef}>
        {appState.chatMessages.value.length !== 0 ? (
          appState.chatMessages.value.map((chatMessage, index) => (
            <li 
              key={index}
              className="border border-secondary list-group-item bg-muted border-2"
            >
              <div className="d-block chat-message">
                <small className="text-muted">{`(${chatMessage.date})`} </small>
                <span className={`${appState.username.value === chatMessage.username ? "text-orange" : "text-primary"}`}>
                  <b>{chatMessage.username}: </b>
                </span>
                <span className="text-dark">{chatMessage.text}</span>
              </div>
            </li>
          ))
        ) : (
          <h6 className="text-white text-center">No messages to display</h6>
        )}
      </div>
    </>
  );
}
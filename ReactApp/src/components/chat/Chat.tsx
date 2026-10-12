import { useContext, useEffect, useRef, useState } from "react";
import { ChatMessage } from "../../types/ChatMessage";
import { BsChatDotsFill, BsSendFill } from "react-icons/bs";
import { AppHubContext, AppStateContext } from "../../context/AppContext";
import { HubMessages } from "../../classes/constants/HubMessages";
import MessageOnChat from "./ChatMessage";
import { InputField } from "../shared/InputField";
import Button from "../shared/Button";

export default function Chat() {
  const appState = useContext(AppStateContext);
  const appHub = useContext(AppHubContext);
  const messagesRef = useRef<HTMLDivElement>(null);
  const [currentChatMessageText, setCurrentChatMessageText] = useState<string>("");

  useEffect(() => {
    if (appState.unreadChatMessagesCount.value !== 0) {
      appState.unreadChatMessagesCount.value = 0;
    }
  }, [appState.unreadChatMessagesCount]);

  useEffect(() => {
    if (messagesRef.current) {
      messagesRef.current.scrollTop = messagesRef.current.scrollHeight;
    }
  }, [appState.chatMessages.value]);

  const handleTextInputChange = (text: string) => {
    setCurrentChatMessageText(text);
  };

  const handleSendMessage = () => {
    if (!currentChatMessageText || currentChatMessageText?.length === 0) {
      return;
    }

    const newChatMessage: ChatMessage = {
      username: appState.username.value,
      text: currentChatMessageText,
      date: new Date(),
    };

    appHub.invoke(HubMessages.AddChatMessage, appState.roomHash.value, newChatMessage);

    setCurrentChatMessageText("");
  };

  const handleEnterPress = (key: string) => {
    if (key === "Enter") {
      handleSendMessage();
    }
  };

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
          appState.chatMessages.value.map((chatMessage: ChatMessage, index: number) => (
            <li key={index} className="chat-message-item border border-secondary list-group-item bg-white text-dark border-2">
              <MessageOnChat chatMessage={chatMessage} />
            </li>
          ))
        ) : (
          <div className="empty-state text-white text-center" role="status">
            <BsChatDotsFill className="empty-state-icon" aria-hidden="true" />
            <span>No chat messages yet</span>
          </div>
        )}
      </div>
    </>
  );
}

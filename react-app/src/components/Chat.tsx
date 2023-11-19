import { useContext, useEffect, useRef, useState } from "react";
import Button from "./Button";
import { ChatMessage } from "../types/ChatMessage";
import { InputForm } from "./InputForm";
import { BsSendFill } from "react-icons/bs";
import { AppStateContext, RoomHubContext } from "../context/RoomHubContext";
import { HubEvents } from "../classes/HubEvents";

export default function Chat() {
  const appState = useContext(AppStateContext);
  const roomHub = useContext(RoomHubContext);

  const messagesRef = useRef<HTMLDivElement>(null);
  const [currentChatMessageText, setCurrentChatMessageText] = useState<string>("");

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

    roomHub.invoke(HubEvents.AddChatMessage, appState.roomHash.value, newChatMessage);

    setCurrentChatMessageText("");
  };

  const handleEnterPress = (key: string) => {
    if (key === "Enter") {
      handleSendMessage();
    }
  }

  return (
    <>
      <div className="d-flex mb-3">
        <InputForm
          classNames="form-control rounded-0"
          value={currentChatMessageText}
          trim={false}
          placeholder="Enter your message"
          isEnabled={true}
          onChange={handleTextInputChange}
          onKeyDown={handleEnterPress}
        />
        <Button
          text={<BsSendFill />}
          classNames="btn btn-primary rounded-0"
          onClick={handleSendMessage}
        />
      </div>
      <div className="list-group rounded-3 control-panel-list" ref={messagesRef}>
      {
        appState.chatMessages.value.length !== 0 ? (
          appState.chatMessages.value.map((chatMessage, index) => (
            <li 
              key={index}
              className="border border-secondary list-group-item bg-muted border-2"
            >
              <div className="d-block chat-message">
                <small className="text-muted">{`(${chatMessage.date})`} </small>
                <span className="text-primary"><b>{chatMessage.username}: </b> </span>
                <span className="text-dark">{chatMessage.text}</span>
              </div>
            </li>
          ))
        ) :
        <h6 className="text-white text-center">No messages to display</h6>
      }
      </div>
    </>
  )
}
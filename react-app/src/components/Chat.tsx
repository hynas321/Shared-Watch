import { useEffect, useRef, useState } from "react";
import Button from "./Button";
import { ChatMessage } from "../types/ChatMessage";
import { TextInput } from "./TextInput";
import { BsSendFill } from "react-icons/bs";

export default function Chat() {
  const [chatMessages, setChatMessages] = useState<ChatMessage[]>([]);
  const [currentChatMessageText, setCurrentChatMessageText] = useState<string>("");
  const messagesRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (messagesRef.current) {
      messagesRef.current.scrollTop = messagesRef.current.scrollHeight;
    }
  }, [chatMessages]);

  const handleTextInputChange = (text: string) => {
    setCurrentChatMessageText(text);
  }

  const handleSendMessage = () => {
    if (!currentChatMessageText || currentChatMessageText?.length === 0) {
      return;
    }

    const newChatMessage: ChatMessage = {
      username: "User1",
      text: currentChatMessageText
    }

    setChatMessages([...chatMessages, newChatMessage])
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
        <TextInput
          value={currentChatMessageText}
          placeholder="Enter your message"
          onChange={handleTextInputChange}
          onKeyDown={handleEnterPress}
        />
        <Button
          text={<BsSendFill />}
          classNames="btn btn-primary"
          onClick={handleSendMessage}
        />
      </div>
      <div className="list-group rounded-3 control-panel-list" ref={messagesRef}>
      {
        chatMessages.length !== 0 ? (
          chatMessages.map((chatMessage, index) => (
            <li 
              key={index}
              className="border border-secondary list-group-item bg-muted border-2"
            >
              <span className="text-primary"><b>{chatMessage.username}</b>: </span>
              <span className="text-dark">{chatMessage.text}</span>
            </li>
          ))
        ) :
        <h6 className="text-white text-center">No messages to display</h6>
      }
      </div>
    </>
  )
}
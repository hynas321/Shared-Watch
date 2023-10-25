import { useEffect, useRef, useState } from "react";
import Button from "./Button";
import { ChatMessage } from "../types/ChatMessage";
import { TextInput } from "./TextInput";
import { BsSendFill } from "react-icons/bs";

export interface ChatProps {
  onChange?: (chatMessages: ChatMessage[]) => void;
}

export default function Chat({onChange}: ChatProps) {
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

    const date = new Date();
    const currentDate: string = `${date.getHours()}:${date.getMinutes()}:${date.getSeconds()}`

    const newChatMessage: ChatMessage = {
      username: "User1",
      text: currentChatMessageText,
      date: currentDate
    }

    setChatMessages([...chatMessages, newChatMessage])
    setCurrentChatMessageText("");
  };

  useEffect(() => {
    if (onChange) {
      onChange(chatMessages);
    }
  }, [chatMessages]);

  const handleEnterPress = (key: string) => {
    if (key === "Enter") {
      handleSendMessage();
    }
  }

  return (
    <>
      <div className="d-flex mb-3">
        <TextInput
          classNames="form-control rounded-0"
          value={currentChatMessageText}
          placeholder="Enter your message"
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
        chatMessages.length !== 0 ? (
          chatMessages.map((chatMessage, index) => (
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
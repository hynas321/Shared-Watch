import { appState } from "../context/AppContext";
import { ChatMessage } from "../types/ChatMessage";

export interface MessageProps {
  chatMessage: ChatMessage;
}

export default function MessageOnChat({ chatMessage }: MessageProps) {
  const formattedTime = formatMessageTime(chatMessage.date);
  const isCurrentUser = isMessageFromCurrentUser(chatMessage.username);
  const usernameClass = isCurrentUser ? "text-orange" : "text-primary";

  return (
    <div className="d-block chat-message">
      <small className="text-muted">{`(${formattedTime})`} </small>
      <span className={usernameClass}>
        <b>{chatMessage.username}: </b>
      </span>
      <span className="text-dark">{chatMessage.text}</span>
    </div>
  );
}

function formatMessageTime(date: string | Date): string {
  return new Date(date).toLocaleTimeString("pl-PL", {
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit",
  });
}

function isMessageFromCurrentUser(username: string): boolean {
  return appState.username.value === username;
}

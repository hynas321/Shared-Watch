import { appState } from "../context/AppContext"
import { ChatMessage } from "../types/ChatMessage"

export interface MessageProps {
  chatMessage: ChatMessage
}

export default function MessageOnChat({chatMessage}: MessageProps) {
  return (
    <div className="d-block chat-message">
      <small className="text-muted">{`(${chatMessage.date})`} </small>
      <span className={`${
        appState.username.value === chatMessage.username ? "text-orange" : "text-primary"}`}
      >
        <b>{chatMessage.username}: </b>
      </span>
      <span className="text-dark">{chatMessage.text}</span>
    </div>
  )
}
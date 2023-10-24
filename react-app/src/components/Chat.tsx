import { ChatMessage } from "../types/ChatMessage"

export interface ChatProps {
  chatMessages: ChatMessage[];
}

export default function Chat({chatMessages}: ChatProps) {
  return (
    <ul className="list-group rounded-3">
    {
      chatMessages.length !== 0 ? (
        chatMessages.map((chatMessage, index) => (
          <li 
            key={index}
            className="border border-secondary list-group-item bg-muted border-2 opacity-75"
          >
            <h5 className="d-inline">{chatMessage.username}</h5>
            <h6 className="text-dark">{chatMessage.text}</h6>
          </li>
        ))
      ) :
      <h6 className="text-white text-center">No messages to display</h6>
    }
  </ul>
  )
}
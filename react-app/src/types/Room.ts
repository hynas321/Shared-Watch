import { RoomType } from "./RoomType"

export type Room = {
  id: number,
  roomName: string,
  hostName: string,
  availableSlots: number,
  totalSlots: number,
  roomType: RoomType,
}
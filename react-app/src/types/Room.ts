import { RoomTypesEnum } from "../enums/RoomTypesEnum"

export type Room = {
  id: number,
  roomName: string,
  hostName: string,
  availableSlots: number,
  totalSlots: number,
  roomType: RoomTypesEnum,
}
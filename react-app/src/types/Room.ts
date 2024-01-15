import { RoomTypesEnum } from "../enums/RoomTypesEnum"

export type Room = {
  roomHash: string,
  roomName: string,
  roomType: RoomTypesEnum,
  occupiedSlots: number,
  totalSlots: number
}
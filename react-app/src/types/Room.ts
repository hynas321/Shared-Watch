import { RoomTypesEnum } from "../enums/RoomTypesEnum"

export type Room = {
  hash: string,
  roomName: string,
  occupiedSlots: number,
  totalSlots: number,
  roomType: RoomTypesEnum,
}
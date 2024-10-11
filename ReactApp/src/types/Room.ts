import { RoomTypesEnum } from "../enums/RoomTypesEnum";

export interface Room {
  roomHash: string;
  roomName: string;
  roomType: RoomTypesEnum;
  occupiedSlots: number;
  totalSlots: number;
}

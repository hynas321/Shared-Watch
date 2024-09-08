import { RoomTypesEnum } from "../enums/RoomTypesEnum";

export interface RoomSettings {
  roomName: string;
  roomPassword: string;
  roomType: RoomTypesEnum;
  maxUsers: number;
}
import { RoomTypesEnum } from "../enums/RoomTypesEnum";

export type RoomSettings = {
  roomName: string;
  roomPassword: string;
  roomType: RoomTypesEnum;
  maxUsers: number;
}
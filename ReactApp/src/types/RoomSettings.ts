import { RoomTypesEnum } from "../enums/RoomTypesEnum";

export interface RoomSettings {
  roomName: string;
  roomType: RoomTypesEnum;
  maxUsers: number;
}

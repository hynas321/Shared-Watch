import { RoomTypesEnum } from "../enums/RoomTypesEnum";

export type RoomNavigationState = {
  roomHash: string;
  roomName: string;
  roomType: RoomTypesEnum;
  password: string;
}
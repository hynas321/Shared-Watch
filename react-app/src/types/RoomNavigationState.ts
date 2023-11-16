import { RoomTypesEnum } from "../enums/RoomTypesEnum";

export type RoomNavigationState = {
  roomName: string;
  roomType: RoomTypesEnum;
  password: string;
}
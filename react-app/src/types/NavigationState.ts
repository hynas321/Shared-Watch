import { RoomTypesEnum } from "../enums/RoomTypesEnum";

export type NavigationState = {
  roomName: string;
  roomType: RoomTypesEnum;
  password: string;
}
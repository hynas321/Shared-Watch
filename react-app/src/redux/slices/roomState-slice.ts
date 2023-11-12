import { PayloadAction, createSlice } from "@reduxjs/toolkit";
import { LocalStorageManager } from "../../classes/LocalStorageManager";
import { RoomTypesEnum } from "../../enums/RoomTypesEnum";

export interface RoomState {
  roomHash: string,
  roomName: string,
  roomType: RoomTypesEnum,
  password: string
}

const localStorageManager = new LocalStorageManager();

const initialState: RoomState = {
  roomHash: "",
  roomName: "",
  roomType: RoomTypesEnum.public,
  password: ""
};

const roomStateSlice = createSlice({
  name: "roomState",
  initialState,
  reducers: {
    updatedRoomHash(state, action: PayloadAction<string>) {
      state.roomHash = action.payload;
      localStorageManager.setUsername(action.payload);
    },
    updatedRoomName(state, action: PayloadAction<string>) {
      state.roomName = action.payload;
    },
    updatedRoomType(state, action: PayloadAction<RoomTypesEnum>) {
      state.roomType = action.payload;
    },
    updatedPassword(state, action: PayloadAction<string>) {
      state.password = action.payload;
    },
    updatedRoomState(state, action: PayloadAction<RoomState>) {
      state.roomHash = action.payload.roomHash;
      state.roomName = action.payload.roomName;
      state.roomType = action.payload.roomType;
      state.password = action.payload.password;
    }
  }
})

export const { updatedRoomHash, updatedRoomName, updatedRoomType, updatedPassword, updatedRoomState } = roomStateSlice.actions;
export default roomStateSlice.reducer;
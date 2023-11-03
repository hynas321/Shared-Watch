import { PayloadAction, createSlice } from "@reduxjs/toolkit";

export interface UserState {
  username: string,
  isAdmin: boolean,
  isInRoom: boolean
};

const initialState: UserState = {
  username: "",
  isAdmin: false,
  isInRoom: false
};

const userStateSlice = createSlice({
  name: "userState",
  initialState,
  reducers: {
    updatedUsername(state, action: PayloadAction<string>) {
      state.username = action.payload;
      localStorage.setItem("username", state.username);
    },
    updatedIsAdmin(state, action: PayloadAction<boolean>) {
      state.isAdmin = action.payload;
    },
    updatedIsInRoom(state, action: PayloadAction<boolean>) {
      state.isInRoom = action.payload;
    }
  }
})

export const { updatedUsername, updatedIsAdmin, updatedIsInRoom } = userStateSlice.actions;
export default userStateSlice.reducer;
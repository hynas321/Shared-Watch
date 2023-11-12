import { configureStore } from '@reduxjs/toolkit';
import userStateReducer from './slices/userState-slice';
import roomStateSlice from './slices/roomState-slice';


export const store = configureStore({
  reducer: {
    userState: userStateReducer,
    roomState: roomStateSlice
  }
})

export type AppDispatch = typeof store.dispatch;
export type RootState = ReturnType<typeof store.getState>;
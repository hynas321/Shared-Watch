import { configureStore } from '@reduxjs/toolkit';
import userStateReducer from './slices/userState-slice';

export const store = configureStore({
  reducer: {
    userState: userStateReducer
  }
})

export type AppDispatch = typeof store.dispatch;
export type RootState = ReturnType<typeof store.getState>;
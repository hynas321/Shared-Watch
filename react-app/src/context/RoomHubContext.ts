import { createContext } from "react";
import { RoomHub } from "../classes/RoomHub";
import { createAppState } from "../state/AppState";

export const roomHub: RoomHub = new RoomHub();
export const RoomHubContext = createContext<RoomHub>(roomHub);

export const appState = createAppState();
export const AppStateContext = createContext(appState);
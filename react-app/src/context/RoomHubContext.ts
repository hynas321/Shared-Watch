import { createContext } from "react";
import { RoomHub } from "../classes/RoomHub";
import { HttpApiEndpoints } from "../classes/HttpApiEndpoints";
import { createAppState } from "../state/AppState";

export const roomHub: RoomHub = new RoomHub(`${"http://localhost:5050"}${HttpApiEndpoints.roomHubConnection}`);
export const RoomHubContext = createContext<RoomHub>(roomHub);

export const appState = createAppState();
export const AppStateContext = createContext(appState);
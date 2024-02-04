import { createContext } from "react";
import { AppHub as AppHub } from "../classes/AppHub";
import { createAppState } from "../state/AppState";

export const appHub: AppHub = new AppHub();
export const AppHubContext = createContext<AppHub>(appHub);

export const appState = createAppState();
export const AppStateContext = createContext(appState);
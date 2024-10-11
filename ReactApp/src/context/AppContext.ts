import { createContext } from "react";
import { SignalRService as SignalRService } from "../classes/services/SignalRService";
import { createAppState } from "../state/AppState";

export const appHub: SignalRService = new SignalRService();
export const AppHubContext = createContext<SignalRService>(appHub);

export const appState = createAppState();
export const AppStateContext = createContext(appState);

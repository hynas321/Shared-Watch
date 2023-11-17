import { createContext } from "react";
import { RoomHub } from "../classes/RoomHub";
import { HttpApiEndpoints } from "../classes/HttpApiEndpoints";

export const roomHub: RoomHub = new RoomHub(`${"http://localhost:5050"}${HttpApiEndpoints.roomHubConnection}`);
export const RoomHubContext = createContext<RoomHub>(roomHub);
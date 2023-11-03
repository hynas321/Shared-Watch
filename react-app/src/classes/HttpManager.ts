import { RoomCreateInput } from "../types/HttpTypes/Input/RoomCreateInput";
import { RoomCreateOutput } from "../types/HttpTypes/Output/RoomCreateOutput";
import { Room } from "../types/Room";
import { HttpApiEndpoints } from "./HttpApiEndpoints";

export class HttpManager {
  httpServerUrl = import.meta.env.SERVER_URL;

  async getAllRooms(): Promise<Room[]> {
    try {
        const response = await fetch(`${"http://localhost:5050"}/${HttpApiEndpoints.getAllRooms}`, {
          method: 'GET',
          headers: {'Content-Type': 'application/json'}
        });
  
        if (response.status !== 200) {
          return [] as Room[];
        } 
  
        return await response.json() as Room[];
    }
    catch (error) {
      return [] as Room[];
    }
  }

  async createRoom(roomName: string, roomPassword: string, username: string): Promise<RoomCreateOutput | null> {
    try {
      const requestBody: RoomCreateInput = {
        roomName: roomName,
        roomPassword: roomPassword,
        username: username
      };

      console.log(requestBody);

      const response = await fetch(`${"http://localhost:5050"}/${HttpApiEndpoints.createRoom}`, {
        method: 'POST',
        body: JSON.stringify(requestBody),
        headers: {'Content-Type': 'application/json'}
      });

      if (response.status !== 201) {
        return null;
      } 

      return await response.json() as RoomCreateOutput;
  }
  catch (error) {
    return null;
  }
  }
}


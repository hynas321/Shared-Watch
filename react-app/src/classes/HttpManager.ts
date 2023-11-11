import axios from "axios";
import { RoomCreateInput } from "../types/HttpTypes/Input/RoomCreateInput";
import { RoomCreateOutput } from "../types/HttpTypes/Output/RoomCreateOutput";
import { Room } from "../types/Room";
import { AuthorizationManager } from "./AuthorizationManager";
import { HttpApiEndpoints } from "./HttpApiEndpoints";
import { RoomJoinInput } from "../types/HttpTypes/Input/RoomJoinInput";
import { RoomJoinOutput } from "../types/HttpTypes/Output/RoomJoinOutput";

export class HttpManager {
  private httpServerUrl = "http://localhost:5050";
  private authorizationManager = new AuthorizationManager();

  async getAllRooms(): Promise<Room[]> {
    try {
        const response = await fetch(`${this.httpServerUrl}/${HttpApiEndpoints.getAllRooms}`, {
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

      const response = await fetch(`${this.httpServerUrl}/${HttpApiEndpoints.createRoom}`, {
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

  async joinRoom(roomHash: string, roomPassword: string, username: string): Promise<RoomJoinOutput | null> {
    try {
      const requestBody: RoomJoinInput = {
        roomPassword: roomPassword,
        username: username
      };

      const response = await axios.post(
        `${this.httpServerUrl}/${HttpApiEndpoints.joinRoom}/${roomHash}`,
        requestBody, {
          headers: {
            'Content-Type': 'application/json'
          },
        }
      );

      if (response.status !== 200) {
        return null;
      }

      return response.data as RoomJoinOutput;

    } catch (error) {
      return null;
    }
  }


  async leaveRoom(roomHash: string): Promise<boolean> {
    try {
      const authorizationToken = this.authorizationManager.getAuthorizationToken();

      const response = await fetch(`${this.httpServerUrl}/${HttpApiEndpoints.leaveRoom}/${roomHash}`, {
        method: 'DELETE',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${authorizationToken}`
        }
      });

      if (response.status !== 200) {
        return false;
      }

      return true;
    }
    catch {
      return false;
    }
  }
}


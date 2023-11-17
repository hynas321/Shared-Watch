import axios from "axios";
import { RoomCreateInput } from "../types/HttpTypes/Input/RoomCreateInput";
import { RoomCreateOutput } from "../types/HttpTypes/Output/RoomCreateOutput";
import { LocalStorageManager } from "./LocalStorageManager";
import { HttpApiEndpoints } from "./HttpApiEndpoints";
import { RoomJoinInput } from "../types/HttpTypes/Input/RoomJoinInput";
import { RoomJoinOutput } from "../types/HttpTypes/Output/RoomJoinOutput";
import { Room } from "../types/Room";
import { RoomExistsOutput } from "../types/HttpTypes/Output/RoomExistsOutput";

export class HttpManager {
  private httpServerUrl = "http://localhost:5050";
  private authorizationManager = new LocalStorageManager();

  async getAllRooms(): Promise<[number, Room[] | undefined]> {
    try {
      const response = await axios.get(
        `${this.httpServerUrl}/${HttpApiEndpoints.getAllRooms}`, {
          headers: {
            'Content-Type': 'application/json'
          },
        }
      );
  
      return [response.status, response.data];
    }
    catch (error: any) {
      return [error.response.status, undefined]
    }
  }

  async createRoom(roomName: string, roomPassword: string, username: string): Promise<[number, RoomCreateOutput | undefined]> {
    try {
      const requestBody: RoomCreateInput = {
        roomName: roomName,
        roomPassword: roomPassword,
        username: username
      };

      const response = await axios.post(
        `${this.httpServerUrl}/${HttpApiEndpoints.createRoom}`,
        requestBody, {
          headers: {
            'Content-Type': 'application/json'
          },
        }
      );

      return [response.status, response.data] as [number, RoomCreateOutput];
    }
    catch (error: any) {
      return [error.response.status, undefined]
    }
  }

  async checkIfRoomExists(roomHash: string): Promise<[number, RoomExistsOutput | undefined]> {
    try {
      const response = await axios.get(
        `${this.httpServerUrl}/${HttpApiEndpoints.roomExists}/${roomHash}`, {
          headers: {
            'Content-Type': 'application/json'
          },
        }
      );
  
      return [response.status, response.data] as [number, RoomExistsOutput]
    }
    catch (error: any) {
      return [error.response.status, undefined];
    }
  }

  async joinRoom(roomHash: string, roomPassword: string, username: string): Promise<[number, RoomJoinOutput | undefined]> {
    try {
      const authorizationToken = this.authorizationManager.getAuthorizationToken();

      const requestBody: RoomJoinInput = {
        roomPassword: roomPassword,
        username: username
      };

      const response = await axios.post(
        `${this.httpServerUrl}/${HttpApiEndpoints.joinRoom}/${roomHash}`,
        requestBody, {
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${authorizationToken}`
          },
        }
      );

      return [response.status, response.data] as [number, RoomJoinOutput]

    } catch (error: any) {
      return [error.response.status, undefined];
    }
  }


  async leaveRoom(roomHash: string): Promise<number> {
    try {
      const authorizationToken = this.authorizationManager.getAuthorizationToken();

      const response = await axios.delete(
        `${this.httpServerUrl}/${HttpApiEndpoints.leaveRoom}/${roomHash}`, {
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${authorizationToken}`
          }
        }
      );

      return response.status as number;

    } catch (error: any) {
      return error.response.status as number;
    }
  }
}

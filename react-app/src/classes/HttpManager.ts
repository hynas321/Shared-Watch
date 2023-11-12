import axios from "axios";
import { RoomCreateInput } from "../types/HttpTypes/Input/RoomCreateInput";
import { RoomCreateOutput } from "../types/HttpTypes/Output/RoomCreateOutput";
import { LocalStorageManager } from "./LocalStorageManager";
import { HttpApiEndpoints } from "./HttpApiEndpoints";
import { RoomJoinInput } from "../types/HttpTypes/Input/RoomJoinInput";
import { RoomJoinOutput } from "../types/HttpTypes/Output/RoomJoinOutput";
import { PromiseOutput as PromiseOutput } from "../types/HttpTypes/PromiseOutput";
import { Room } from "../types/Room";

export class HttpManager {
  private httpServerUrl = "http://localhost:5050";
  private authorizationManager = new LocalStorageManager();

  async getAllRooms(): Promise<PromiseOutput> {
    try {
      const response = await axios.get(
        `${this.httpServerUrl}/${HttpApiEndpoints.getAllRooms}`, {
          headers: {
            'Content-Type': 'application/json'
          },
        }
      );

      const output: PromiseOutput = {
        status: response.status as number,
        data: response.data as Room[]
      }
  
      return output as PromiseOutput;
    }
    catch (error: any) {
      return { status: error.response.status, data: undefined } as PromiseOutput;
    }
  }

  async createRoom(roomName: string, roomPassword: string, username: string): Promise<PromiseOutput> {
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

      const output: PromiseOutput = {
        status: response.status as number,
        data: response.data as RoomCreateOutput
      }

      return output as PromiseOutput;
    }
    catch (error: any) {
      return { status: error.response.status as number, data: undefined } as PromiseOutput;
    }
  }

  async joinRoom(roomHash: string, roomPassword: string, username: string): Promise<PromiseOutput> {
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
      
      const output: PromiseOutput = {
        status: response.status as number,
        data: response.data as RoomJoinOutput
      }

      return output as PromiseOutput;

    } catch (error: any) {
      return { status: error.response.status as number, data: undefined } as PromiseOutput;
    }
  }


  async leaveRoom(roomHash: string): Promise<PromiseOutput> {
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

      const output: PromiseOutput = {
        status: response.status as number,
        data: response.data as RoomJoinOutput
      }

      return output as PromiseOutput;

    } catch (error: any) {
      return { status: error.response.status as number, data: undefined } as PromiseOutput;
    }
  }
}


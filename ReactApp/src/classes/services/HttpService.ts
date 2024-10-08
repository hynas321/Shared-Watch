import axios from "axios";
import { RoomCreateInput } from "../../types/HttpTypes/Input/RoomCreateInput";
import { RoomCreateOutput } from "../../types/HttpTypes/Output/RoomCreateOutput";
import { LocalStorageService } from "./LocalStorageService";
import { HttpApiEndpoints } from "../constants/HttpApiEndpoints";
import { RoomJoinInput } from "../../types/HttpTypes/Input/RoomJoinInput";
import { RoomJoinOutput } from "../../types/HttpTypes/Output/RoomJoinOutput";
import { Room } from "../../types/Room";
import { appState } from "../../context/AppContext";

export class HttpService {
  private httpServerUrl: string;
  private authorizationManager = LocalStorageService.getInstance();

  private static instance: HttpService;

  private constructor() {
    let env = import.meta.env;
    this.httpServerUrl = env.VITE_SERVER_URL;
  }

  static getInstance(): HttpService {
    if (!HttpService.instance) {
      HttpService.instance = new HttpService();
    }
    return HttpService.instance;
  }

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
      if (error.response === undefined) {
        return [500, undefined];
      }

      return [error.response.status, undefined]
    }
  }

  async createRoom(roomName: string, roomPassword: string, username: string): Promise<[number, RoomCreateOutput | undefined]> {
    try {
      const authorizationToken = this.authorizationManager.getAuthorizationToken();

      const requestBody: RoomCreateInput = {
        roomName: roomName,
        roomPassword: roomPassword,
        username: username
      };

      const response = await axios.post(
        `${this.httpServerUrl}/${HttpApiEndpoints.createRoom}`,
        requestBody, {
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `${authorizationToken}`,
            'X-SignalR-ConnectionId': appState.connectionId.value
          },
        }
      );

      return [response.status, response.data] as [number, RoomCreateOutput];
    }
    catch (error: any) {
      if (error.response === undefined) {
        return [500, undefined];
      }

      return [error.response.status, undefined]
    }
  }

  async getRoom(roomHash: string): Promise<[number, Room | undefined]> {
    try {
      const response = await axios.get(
        `${this.httpServerUrl}/${HttpApiEndpoints.getRoom}/${roomHash}`, {
          headers: {
            'Content-Type': 'application/json'
          },
        }
      );
  
      return [response.status, response.data] as [number, Room]
    }
    catch (error: any) {
      if (error.response === undefined) {
        return [500, undefined];
      }

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
            'Authorization': `${authorizationToken}`,
            'X-SignalR-ConnectionId': appState.connectionId.value
          },
        }
      );

      return [response.status, response.data] as [number, RoomJoinOutput]

    } catch (error: any) {
      if (error.response === undefined) {
        return [500, undefined];
      }

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
            'Authorization': `${authorizationToken}`,
            'X-SignalR-ConnectionId': appState.connectionId.value
          }
        }
      );

      return response.status as number;

    } catch (error: any) {
      if (error.response === undefined) {
        return 500;
      }

      return error.response.status;
    }
  }
}


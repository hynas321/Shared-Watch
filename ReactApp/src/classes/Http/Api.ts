import axios, { HttpStatusCode } from "axios";
import { RoomCreateInput } from "../../types/HttpTypes/Input/RoomCreateInput";
import { RoomCreateOutput } from "../../types/HttpTypes/Output/RoomCreateOutput";
import { RoomJoinInput } from "../../types/HttpTypes/Input/RoomJoinInput";
import { RoomJoinOutput } from "../../types/HttpTypes/Output/RoomJoinOutput";
import { Room } from "../../types/Room";
import { HttpApiEndpoints } from "../constants/HttpApiEndpoints";

const env = import.meta.env;
const httpServerUrl = env.VITE_SERVER_URL;

const api = {
  async getAllRooms(): Promise<[number, Room[] | undefined]> {
    try {
      const response = await axios.get(`${httpServerUrl}/${HttpApiEndpoints.getAllRooms}`, {
        headers: { "Content-Type": "application/json" },
      });
      return [response.status, response.data];
    } catch (error: any) {
      if (error.response === undefined) return [HttpStatusCode.InternalServerError, undefined];
      return [error.response.status, undefined];
    }
  },

  async createRoom(
    roomName: string,
    roomPassword: string,
    username: string
  ): Promise<[number, RoomCreateOutput | undefined]> {
    try {
      const authorizationToken = sessionStorage.getItem("authorizationToken") ?? "";

      const requestBody: RoomCreateInput = {
        roomName, roomPassword, username
      };

      const response = await axios.post(
        `${httpServerUrl}/${HttpApiEndpoints.createRoom}`,
        requestBody,
        {
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${authorizationToken}`,
          },
        }
      );

      return [response.status, response.data];
    } catch (error: any) {
      if (error.response === undefined) return [500, undefined];
      return [error.response.status, undefined];
    }
  },

  async getRoom(roomHash: string): Promise<[number, Room | undefined]> {
    try {
      const response = await axios.get(
        `${httpServerUrl}/${HttpApiEndpoints.getRoom}/${roomHash}`,
        { headers: { "Content-Type": "application/json" } }
      );
      return [response.status, response.data];
    } catch (error: any) {
      if (error.response === undefined) return [500, undefined];
      return [error.response.status, undefined];
    }
  },

  async joinRoom(
    roomHash: string,
    roomPassword: string,
    username: string
  ): Promise<[number, RoomJoinOutput | undefined]> {
    try {
      const authorizationToken = sessionStorage.getItem("authorizationToken") ?? "";

      const requestBody: RoomJoinInput = { roomPassword, username };

      const response = await axios.post(
        `${httpServerUrl}/${HttpApiEndpoints.joinRoom}/${roomHash}`,
        requestBody,
        {
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${authorizationToken}`,
          },
        }
      );

      return [response.status, response.data];
    } catch (error: any) {
      if (error.response === undefined) return [500, undefined];
      return [error.response.status, undefined];
    }
  },

  async leaveRoom(roomHash: string): Promise<number> {
    try {
      const authorizationToken = sessionStorage.getItem("authorizationToken") ?? "";

      const response = await axios.delete(
        `${httpServerUrl}/${HttpApiEndpoints.leaveRoom}/${roomHash}`,
        {
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${authorizationToken}`,
          },
        }
      );

      return response.status;
    } catch (error: any) {
      if (error.response === undefined) return 500;
      return error.response.status;
    }
  },
};

export default api;

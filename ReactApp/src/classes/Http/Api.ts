import axios from "axios";
import { RoomCreateInput } from "../../types/HttpTypes/Input/RoomCreateInput";
import { RoomCreateOutput } from "../../types/HttpTypes/Output/RoomCreateOutput";
import { RoomJoinInput } from "../../types/HttpTypes/Input/RoomJoinInput";
import { RoomJoinOutput } from "../../types/HttpTypes/Output/RoomJoinOutput";
import { Room } from "../../types/Room";
import { PlaylistVideo } from "../../types/PlaylistVideo";
import { HttpApiEndpoints } from "../constants/HttpApiEndpoints";

const httpServerUrl = (import.meta.env.VITE_SERVER_URL as string | undefined)?.replace(/\/$/, "") ?? "";
const jsonHeaders = { "Content-Type": "application/json" } as const;
const HTTP_REQUEST_TIMEOUT_MS = 5_000;
const VIDEO_UPLOAD_TIMEOUT_MS = 10 * 60 * 1_000;

axios.defaults.withCredentials = true;

export const NetworkErrorStatus = 0;

type ApiResult<T> = [status: number, data: T | undefined];

const getAuthorizationHeaders = () => ({ ...jsonHeaders });

const apiUrl = (endpoint: string): string => `${httpServerUrl}/${endpoint}`;

const getErrorStatus = (error: unknown): number =>
  axios.isAxiosError(error)
    ? error.response?.status ?? NetworkErrorStatus
    : NetworkErrorStatus;

const api = {
  async healthCheck(): Promise<number> {
    try {
      const response = await axios.get(apiUrl(HttpApiEndpoints.health), {
        timeout: HTTP_REQUEST_TIMEOUT_MS,
      });
      return response.status;
    } catch (error: unknown) {
      return getErrorStatus(error);
    }
  },

  async getAllRooms(): Promise<ApiResult<Room[]>> {
    try {
      const response = await axios.get(apiUrl(HttpApiEndpoints.getAllRooms), {
        headers: jsonHeaders,
        timeout: HTTP_REQUEST_TIMEOUT_MS,
      });
      return [response.status, response.data];
    } catch (error: unknown) {
      return [getErrorStatus(error), undefined];
    }
  },

  async createRoom(
    roomName: string,
    roomPassword: string,
    username: string
  ): Promise<ApiResult<RoomCreateOutput>> {
    try {
      const requestBody: RoomCreateInput = {
        roomName, roomPassword, username
      };

      const response = await axios.post(
        apiUrl(HttpApiEndpoints.createRoom),
        requestBody,
        {
          headers: {
            ...getAuthorizationHeaders(),
          },
          timeout: HTTP_REQUEST_TIMEOUT_MS,
        }
      );

      return [response.status, response.data];
    } catch (error: unknown) {
      return [getErrorStatus(error), undefined];
    }
  },

  async getRoom(roomHash: string): Promise<ApiResult<Room>> {
    try {
      const response = await axios.get(
        apiUrl(`${HttpApiEndpoints.getRoom}/${encodeURIComponent(roomHash)}`),
        { headers: jsonHeaders, timeout: HTTP_REQUEST_TIMEOUT_MS }
      );
      return [response.status, response.data];
    } catch (error: unknown) {
      return [getErrorStatus(error), undefined];
    }
  },

  async joinRoom(
    roomHash: string,
    roomPassword: string,
    username: string
  ): Promise<ApiResult<RoomJoinOutput>> {
    try {
      const requestBody: RoomJoinInput = { roomPassword, username };

      const response = await axios.post(
        apiUrl(`${HttpApiEndpoints.joinRoom}/${encodeURIComponent(roomHash)}`),
        requestBody,
        {
          headers: {
            ...getAuthorizationHeaders(),
          },
          timeout: HTTP_REQUEST_TIMEOUT_MS,
        }
      );

      return [response.status, response.data];
    } catch (error: unknown) {
      return [getErrorStatus(error), undefined];
    }
  },

  async leaveRoom(roomHash: string): Promise<number> {
    try {
      const response = await axios.delete(
        apiUrl(`${HttpApiEndpoints.leaveRoom}/${encodeURIComponent(roomHash)}`),
        {
          headers: {
            ...getAuthorizationHeaders(),
          },
          timeout: HTTP_REQUEST_TIMEOUT_MS,
        }
      );

      return response.status;
    } catch (error: unknown) {
      return getErrorStatus(error);
    }
  },

  async refreshAuthorizationToken(): Promise<number> {
    try {
      const response = await axios.post(
        apiUrl(HttpApiEndpoints.refreshToken),
        undefined,
        { timeout: HTTP_REQUEST_TIMEOUT_MS }
      );
      return response.status;
    } catch (error: unknown) {
      return getErrorStatus(error);
    }
  },

  async uploadVideo(
    roomHash: string,
    file: File,
    duration: number,
    onProgress: (progress: number) => void
  ): Promise<[status: number, data: PlaylistVideo | undefined, errorMessage: string | undefined]> {
    try {
      const formData = new FormData();
      formData.append("file", file);
      formData.append("duration", duration.toString());

      const response = await axios.post(
        apiUrl(`api/VideoFiles/${encodeURIComponent(roomHash)}`),
        formData,
        {
          withCredentials: true,
          timeout: VIDEO_UPLOAD_TIMEOUT_MS,
          onUploadProgress: (event) => {
            if (event.total) {
              onProgress(Math.round((event.loaded / event.total) * 100));
            }
          },
        }
      );
      return [response.status, response.data, undefined];
    } catch (error: unknown) {
      const errorMessage = axios.isAxiosError(error) && typeof error.response?.data?.message === "string"
        ? error.response.data.message
        : axios.isAxiosError(error) && error.code === "ECONNABORTED"
          ? "The upload timed out. Check the file size and your connection, then try again."
          : undefined;
      return [getErrorStatus(error), undefined, errorMessage];
    }
  },

  async getVideoStreamUrl(roomHash: string, fileName: string): Promise<string> {
    const response = await axios.get(
      apiUrl(`api/VideoFiles/${encodeURIComponent(roomHash)}/${encodeURIComponent(fileName)}/access`),
      { headers: getAuthorizationHeaders(), timeout: HTTP_REQUEST_TIMEOUT_MS }
    );
    return response.data.url as string;
  },
};

export default api;

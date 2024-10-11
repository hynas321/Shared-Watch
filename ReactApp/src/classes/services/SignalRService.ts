import * as signalR from "@microsoft/signalr";
import { HttpApiEndpoints } from "../constants/HttpApiEndpoints";
import { SessionStorageService } from "./SessionStorageService";

export class SignalRService {
  private connection: signalR.HubConnection;
  private httpWebSocketUrl: string;
  private sessionStorage = SessionStorageService.getInstance();

  constructor() {
    let env = import.meta.env;
    this.httpWebSocketUrl = env.VITE_WEBSOCKET_URL;

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${this.httpWebSocketUrl}${HttpApiEndpoints.appHubConnection}`, {
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets,
        withCredentials: false,
        accessTokenFactory: () => this.sessionStorage.getAuthorizationToken(),
      })
      .withAutomaticReconnect()
      .build();
  }

  async start() {
    return await this.connection.start();
  }

  async stop() {
    return await this.connection.stop();
  }

  async send(name: string, ...args: any[]) {
    return await this.connection.send(name, ...args);
  }

  async invoke(name: string, ...args: any[]) {
    return await this.connection.invoke(name, ...args);
  }

  on(name: string, callback: (...args: any[]) => any) {
    this.connection.on(name, callback);
  }

  off(name: string) {
    this.connection.off(name);
  }

  getState() {
    return this.connection.state;
  }
}

import * as signalR from "@microsoft/signalr";

export class RoomHub {
  private connection: signalR.HubConnection

  constructor(url: string) {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(url, {
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets,
        withCredentials: false
      })
      .withAutomaticReconnect()
      .build();
  }

  async start() {
    if (this.connection.state === signalR.HubConnectionState.Disconnected) {
      return await this.connection.start();
    }
  }

  async stop() {
    if (this.connection.state === signalR.HubConnectionState.Connected) {
      return await this.connection.stop();
    }
  }

  async send(name: string, ...args: any[]) {
    if (this.connection.state === signalR.HubConnectionState.Connected) {
      return await this.connection.send(name, ...args);
    }
  }

  async invoke(name: string, ...args: any[]) {
    if (this.connection.state === signalR.HubConnectionState.Connected) {
      return await this.connection.invoke(name, ...args);
    }
  }

  on(name: string, callback: (...args: any[]) => any) {
    this.connection.on(name, callback);
  }

  off(name: string) {
    this.connection.off(name);
  }

  async onclose(callback: (...args: any[]) => any) {
    await this.connection.onclose(callback);
  }

  onreconnected(callback: (...args: any[]) => any) {
    this.connection.onreconnected(callback);
  }

  getConnection() {
    return this.connection;
  }

  getState() {
    return this.connection.state;
  }
}
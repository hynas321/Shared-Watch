import * as signalR from "@microsoft/signalr";
import { HttpApiEndpoints } from "../constants/HttpApiEndpoints";

export class SignalRService {
  private connection: signalR.HubConnection;
  private httpWebSocketUrl: string;
  private stateChangeListeners = new Set<() => void>();
  private requestFailureListeners = new Set<(error: unknown) => void>();
  private eventHandlers = new WeakMap<object, (...args: unknown[]) => void>();

  constructor() {
    const env = import.meta.env;
    this.httpWebSocketUrl = env.VITE_WEBSOCKET_URL.replace(/^ws(s?):\/\//i, "http$1://");

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${this.httpWebSocketUrl}${HttpApiEndpoints.appHubConnection}`, {
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets,
        withCredentials: true,
      })
      .withAutomaticReconnect()
      .build();

    this.connection.onreconnecting(() => this.notifyStateChanged());
    this.connection.onreconnected(() => this.notifyStateChanged());
    this.connection.onclose(() => this.notifyStateChanged());
  }

  async start() {
    try {
      await this.connection.start();
    } finally {
      this.notifyStateChanged();
    }
  }

  async stop() {
    try {
      await this.connection.stop();
    } finally {
      this.notifyStateChanged();
    }
  }

  async send(name: string, ...args: unknown[]): Promise<void> {
    try {
      return await this.connection.send(name, ...args);
    } catch (error) {
      this.notifyRequestFailure(error);
      throw error;
    }
  }

  async invoke<TResult = void>(name: string, ...args: unknown[]): Promise<TResult> {
    try {
      return await this.connection.invoke<TResult>(name, ...args);
    } catch (error) {
      this.notifyRequestFailure(error);
      throw error;
    }
  }

  on<TArgs extends unknown[]>(name: string, callback: (...args: TArgs) => void) {
    const handler = (...args: unknown[]) => callback(...(args as TArgs));
    this.eventHandlers.set(callback, handler);
    this.connection.on(name, handler);
  }

  off<TArgs extends unknown[]>(name: string, callback?: (...args: TArgs) => void) {
    if (callback) {
      const handler = this.eventHandlers.get(callback);
      if (handler) {
        this.connection.off(name, handler);
        this.eventHandlers.delete(callback);
      }
      return;
    }
    this.connection.off(name);
  }

  getState() {
    return this.connection.state;
  }

  onStateChanged(listener: () => void) {
    this.stateChangeListeners.add(listener);
    return () => {
      this.stateChangeListeners.delete(listener);
    };
  }

  onRequestFailed(listener: (error: unknown) => void) {
    this.requestFailureListeners.add(listener);
    return () => {
      this.requestFailureListeners.delete(listener);
    };
  }

  private notifyStateChanged() {
    this.stateChangeListeners.forEach((listener) => listener());
  }

  private notifyRequestFailure(error: unknown) {
    this.requestFailureListeners.forEach((listener) => listener(error));
  }
}

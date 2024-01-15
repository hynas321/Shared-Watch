export class LocalStorageManager {
  static authorizationTokenKey: string = "authorizationToken";
  static usernameKey: string = "username";

  setAuthorizationToken(value: string): void {
    localStorage.setItem(LocalStorageManager.authorizationTokenKey, value)
  }

  getAuthorizationToken(): string {
    return localStorage.getItem(LocalStorageManager.authorizationTokenKey) ?? "";
  }

  setUsername(value: string): void {
    localStorage.setItem(LocalStorageManager.usernameKey, value);
  }

  getUsername(): string {
    return localStorage.getItem(LocalStorageManager.usernameKey) ?? "";
  }
}
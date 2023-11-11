export class AuthorizationManager {
  static authorizationTokenName = "authorizationToken";

  setAuthorizationToken(value: string): void {
    localStorage.setItem(AuthorizationManager.authorizationTokenName, value)
  }

  getAuthorizationToken(): string {
    return localStorage.getItem(AuthorizationManager.authorizationTokenName) ?? "";
  }
}
export class SessionStorageService {
  static authorizationTokenKey: string = "authorizationToken";
  static usernameKey: string = "username";

  private static instance: SessionStorageService;

  private constructor() {}

  static getInstance(): SessionStorageService {
    if (!SessionStorageService.instance) {
      SessionStorageService.instance = new SessionStorageService();
    }
    return SessionStorageService.instance;
  }

  setAuthorizationToken(value: string): void {
    sessionStorage.setItem(SessionStorageService.authorizationTokenKey, value);
  }

  getAuthorizationToken(): string {
    return sessionStorage.getItem(SessionStorageService.authorizationTokenKey) ?? "";
  }

  clearAuthorizationToken(): void {
    return sessionStorage.setItem(SessionStorageService.authorizationTokenKey, "");
  }

  setUsername(value: string): void {
    sessionStorage.setItem(SessionStorageService.usernameKey, value);
  }

  getUsername(): string {
    return sessionStorage.getItem(SessionStorageService.usernameKey) ?? "";
  }
}

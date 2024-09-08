export class LocalStorageService {
  static authorizationTokenKey: string = "authorizationToken";
  static usernameKey: string = "username";

  private static instance: LocalStorageService;

  private constructor() {}

  static getInstance(): LocalStorageService {
    if (!LocalStorageService.instance) {
      LocalStorageService.instance = new LocalStorageService();
    }
    return LocalStorageService.instance;
  }

  setAuthorizationToken(value: string): void {
    localStorage.setItem(LocalStorageService.authorizationTokenKey, value)
  }

  getAuthorizationToken(): string {
    return localStorage.getItem(LocalStorageService.authorizationTokenKey) ?? "";
  }

  setUsername(value: string): void {
    localStorage.setItem(LocalStorageService.usernameKey, value);
  }

  getUsername(): string {
    return localStorage.getItem(LocalStorageService.usernameKey) ?? "";
  }
}
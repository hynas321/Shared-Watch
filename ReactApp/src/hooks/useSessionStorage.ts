import { useReducer } from "react";

export const useSessionStorage = () => {
  const [, forceUpdate] = useReducer((x) => x + 1, 0);

  const username = sessionStorage.getItem("username") ?? "";
  const authorizationToken = sessionStorage.getItem("authorizationToken") ?? "";

  const setAuthorizationToken = (value: string): void => {
    sessionStorage.setItem("authorizationToken", value);
    forceUpdate();
  };

  const clearAuthorizationToken = (): void => {
    sessionStorage.setItem("authorizationToken", "");
    forceUpdate();
  };

  const setUsername = (value: string): void => {
    sessionStorage.setItem("username", value);
    forceUpdate();
  };

  return {
    authorizationToken,
    setAuthorizationToken,
    clearAuthorizationToken,
    username,
    setUsername,
  };
};

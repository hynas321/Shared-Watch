import { useCallback, useReducer } from "react";

export const useSessionStorage = () => {
  const [, forceUpdate] = useReducer((x) => x + 1, 0);

  const username = sessionStorage.getItem("username") ?? "";
  const setUsername = useCallback((value: string): void => {
    sessionStorage.setItem("username", value);
    forceUpdate();
  }, []);

  return {
    username,
    setUsername,
  };
};

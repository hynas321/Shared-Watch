const serverUrl = (import.meta.env.VITE_SERVER_URL as string | undefined)?.replace(/\/$/, "") ?? "";

export const getPlayableVideoUrl = (url: string): string =>
  url.startsWith("/api/VideoFiles/") ? `${serverUrl}${url}` : url;

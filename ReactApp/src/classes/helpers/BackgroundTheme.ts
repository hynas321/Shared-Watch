export type BackgroundTheme = "colorful" | "black";

const storageKey = "backgroundTheme";

export const getBackgroundTheme = (): BackgroundTheme =>
  localStorage.getItem(storageKey) === "black" ? "black" : "colorful";

export const applyBackgroundTheme = (theme: BackgroundTheme) => {
  document.body.classList.remove("background-gradient", "background-dark");
  document.body.classList.add(theme === "black" ? "background-dark" : "background-gradient");
};

export const saveBackgroundTheme = (theme: BackgroundTheme) => {
  localStorage.setItem(storageKey, theme);
  applyBackgroundTheme(theme);
};

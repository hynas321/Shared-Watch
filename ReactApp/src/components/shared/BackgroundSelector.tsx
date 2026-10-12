import { useEffect, useState } from "react";
import {
  applyBackgroundTheme,
  getBackgroundTheme,
  saveBackgroundTheme,
  type BackgroundTheme,
} from "../../classes/helpers/BackgroundTheme";

export default function BackgroundSelector() {
  const [theme, setTheme] = useState<BackgroundTheme>(() => getBackgroundTheme());

  useEffect(() => {
    applyBackgroundTheme(theme);
  }, [theme]);

  const changeTheme = (event: React.ChangeEvent<HTMLSelectElement>) => {
    const nextTheme = event.target.value as BackgroundTheme;
    setTheme(nextTheme);
    saveBackgroundTheme(nextTheme);
  };

  return (
    <label className="background-selector d-flex align-items-center gap-2">
      <span className="text-white small">Background</span>
      <select className="form-select form-select-sm" value={theme} onChange={changeTheme} aria-label="Background">
        <option value="colorful">Colorful</option>
        <option value="black">Black</option>
      </select>
    </label>
  );
}

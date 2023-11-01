import React, { useState } from 'react';

export interface SwitchProps {
  label: string;
  defaultIsChecked: boolean;
  isEnabled: boolean;
  onCheckChange: (checked: boolean) => void;
}

export default function Switch({ label, defaultIsChecked, isEnabled, onCheckChange }: SwitchProps) {
  const [isChecked, setIsChecked] = useState(defaultIsChecked);

  const handleCheckChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const newChecked = event.target.checked;
    setIsChecked(newChecked);
    onCheckChange(newChecked);
  };

  return (
    <div className="form-check form-switch">
      <input
        className="form-check-input"
        type="checkbox"
        role="switch"
        id="flexSwitchCheckChecked"
        checked={isChecked}
        disabled={!isEnabled}
        onChange={handleCheckChange}
      />
      <label className="form-check-label text-white" htmlFor="flexSwitchCheckChecked">
        {label}
      </label>
    </div>
  );
}
export interface SwitchProps {
  label: string;
  isChecked: boolean;
  isEnabled: boolean;
  onCheckChange: (checked: boolean) => void;
}

export default function Switch({ label, isChecked, isEnabled, onCheckChange }: SwitchProps) {
  const handleCheckChange = () => {
    onCheckChange(!isChecked);
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

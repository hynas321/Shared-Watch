import { forwardRef } from "react";

export interface InputFormProps {
  classNames: string;
  placeholder: string;
  value: string;
  trim: boolean;
  isEnabled: boolean;
  onChange: (value: string) => void;
  onKeyDown?: (key: string) => void;
};

export const InputForm = forwardRef(
  ({ classNames, placeholder, value, trim, isEnabled, onChange, onKeyDown}: InputFormProps,
    ref: React.ForwardedRef<HTMLInputElement>
  ) => {
    const handleInputChange = (event: React.ChangeEvent<HTMLInputElement>) => {
      onChange(trim ? event.target.value.trim() : event.target.value);
    }

    const handleKeyDown = (event: React.KeyboardEvent<HTMLInputElement>) => {
      if (onKeyDown != null) {
        onKeyDown(event.key);
      }
    }

    return (
      <input
        className={`${classNames} ${isEnabled ? "available-element" : "unavailable-element"}`}
        ref={ref}
        type="text"
        value={value} 
        placeholder={placeholder}
        onChange={handleInputChange}
        onKeyDown={handleKeyDown}
        disabled={!isEnabled}
      />
    );
  }
);
import { forwardRef } from "react";

export interface TextInputProps {
  placeholder: string;
  value: string;
  onChange: (value: string) => void;
  onKeyDown: (key: string) => void;
};

export const TextInput = forwardRef(
  ({ placeholder, value, onChange, onKeyDown }: TextInputProps,
    ref: React.ForwardedRef<HTMLInputElement>
  ) => {
    const handleInputChange = (event: React.ChangeEvent<HTMLInputElement>) => {
      onChange(event.target.value);
    }

    const handleKeyDown = (event: React.KeyboardEvent<HTMLInputElement>) => {
      if (onKeyDown != null) {
        onKeyDown(event.key);
      }
    }

    return (
      <input
        className="form-control"
        ref={ref}
        type="text"
        value={value} 
        placeholder={placeholder}
        onChange={handleInputChange}
        onKeyDown={handleKeyDown}
      />
    );
  }
);
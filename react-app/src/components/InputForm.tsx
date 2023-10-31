import { forwardRef } from "react";

export interface InputFormProps {
  classNames: string;
  placeholder: string;
  value: string;
  onChange: (value: string) => void;
  onKeyDown?: (key: string) => void;
};

export const InputForm = forwardRef(
  ({ classNames, placeholder, value, onChange, onKeyDown }: InputFormProps,
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
        className={classNames}
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
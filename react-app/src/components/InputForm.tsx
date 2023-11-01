import { forwardRef } from "react";

export interface InputFormProps {
  classNames: string;
  placeholder: string;
  value: string;
  onChange: (value: string) => void;
  onKeyDown?: (key: string) => void;
  onClick?: (event: any) => void;
};

export const InputForm = forwardRef(
  ({ classNames, placeholder, value, onChange, onKeyDown, onClick }: InputFormProps,
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

    const handleOnClick = (event: any) => {
      if (onClick != null) {
        onClick(event);
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
        onClick={handleOnClick}
      />
    );
  }
);
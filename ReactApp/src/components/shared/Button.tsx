import { ReactNode } from "react";

export interface ButtonProps {
  text: string | ReactNode;
  classNames: string;
  onClick: React.MouseEventHandler<HTMLButtonElement>;
  disabled?: boolean;
}

export default function Button({ text, classNames, onClick, disabled = false }: ButtonProps) {
  return (
    <button type="button" className={classNames} onClick={onClick} disabled={disabled}>
      {text}
    </button>
  );
}

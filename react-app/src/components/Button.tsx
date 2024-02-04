import { ReactNode } from "react";

export interface ButtonProps {
    text: string | ReactNode,
    classNames: string,
    onClick: (any: any) => void;
}

export default function Button({text, classNames, onClick}: ButtonProps) {
  return (
    <button className={classNames} onClick={onClick}>{text}</button>
  )
}

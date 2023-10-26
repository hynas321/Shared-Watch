import { ReactNode } from "react";

export interface ButtonProps {
    text: string | ReactNode,
    classNames: string,
    styles?: object,
    onClick: (any: any) => void;
}

export default function Button({text, classNames, styles, onClick}: ButtonProps) {
  return (
    <button
      className={classNames}
      style={styles}
      onClick={onClick}
    >
      {text}
    </button>
  )
}

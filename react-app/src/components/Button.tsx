import { ReactNode } from "react";

export interface ButtonProps {
    text: string | ReactNode,
    bootstrapClass: string
    styles?: object,
    onClick: (any: any) => void;
}

export default function Button({text, bootstrapClass, styles, onClick}: ButtonProps) {
  return (
    <button
      className={`btn ${bootstrapClass}`}
      style={styles}
      onClick={onClick}
    >
      {text}
    </button>
  )
}

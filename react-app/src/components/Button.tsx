export interface ButtonProps {
    text: string,
    bootstrapClass: string
    styles: object,
    onClick: () => void;
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

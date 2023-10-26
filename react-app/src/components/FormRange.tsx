import { useEffect, useState } from "react";

export interface RangeProps {
  label: string;
  labelClassNames: string;
  minValue: number;
  maxValue: number;
  defaultValue: number;
  step: number;
  onChange: (value: number) => void;
}

export default function FormRange({label, labelClassNames, minValue, maxValue, defaultValue, step, onChange}: RangeProps) {
  const [inputValue, setInputValue] = useState<number>(defaultValue);

  const handleInputValueChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setInputValue(parseInt(event.target.value));
  }

  useEffect(() => {
    onChange(inputValue);
  }, [inputValue]);

  return (
    <>
      <label htmlFor="range-component" className={labelClassNames}>{label} {inputValue}</label>
      <input
        id="range-component"
        type="range"
        className="form-range"
        min={minValue}
        max={maxValue}
        defaultValue={defaultValue}
        step={step}
        onChange={handleInputValueChange}
      />
    </>
  )
}

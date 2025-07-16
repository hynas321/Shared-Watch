import { BsCameraVideoOffFill } from "react-icons/bs";

export default function NoVideoPlaceholder({ isMobileView }: { isMobileView: boolean }) {
  return (
    <div
      className="d-flex align-items-center justify-content-center text-white"
      style={{
        width: isMobileView ? "428px" : "854px",
        height: isMobileView ? "auto" : "480px",
      }}
    >
      <div className="text-center">
        <h1><BsCameraVideoOffFill /></h1>
        <h5>No video to display</h5>
      </div>
    </div>
  );
}

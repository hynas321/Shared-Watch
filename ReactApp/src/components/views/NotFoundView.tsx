import { useNavigate } from "react-router-dom";
import Button from "../shared/Button";
import { ClientEndpoints } from "../../classes/constants/ClientEndpoints";

export default function NotFoundView() {
  const navigate = useNavigate();

  return (
    <div className="container">
      <div className="row">
        <h2 className="text-center text-white mt-5">
          <b>{"Page not found :("}</b>
        </h2>
        <div className="d-flex justify-content-center">
          <Button
            text="Go to Main Menu"
            classNames={`btn btn-primary mx-1 mt-3`}
            onClick={() => navigate(`${ClientEndpoints.mainMenu}`)}
          />
        </div>
      </div>
    </div>
  );
}

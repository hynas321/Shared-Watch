import CreateRoomForm from "./CreateRoomForm";
import Button from "../shared/Button";

export interface CreateRoomModalProps {
  acceptText: string;
  declineText: string;
  isEnabled: boolean;
}

export default function CreateRoomModal({
  acceptText,
  declineText,
  isEnabled,
}: CreateRoomModalProps) {
  return (
    <>
      <div className="rounded-1" data-bs-toggle="modal" data-bs-target="#exampleModal">
        <Button
          text="Create Room"
          classNames={`btn btn-success ${!isEnabled && "disabled"}`}
          onClick={() => {}}
        />
      </div>

      <div
        className="modal fade"
        id="exampleModal"
        tabIndex={-1}
        role="dialog"
        style={{ marginTop: "3.75rem", backgroundColor: "rgba(0,0,0,.0001)" }}
      >
        <div className="modal-dialog" role="document">
          <div className="modal-content">
            <CreateRoomForm acceptText={acceptText} declineText={declineText} />
          </div>
        </div>
      </div>
    </>
  );
}

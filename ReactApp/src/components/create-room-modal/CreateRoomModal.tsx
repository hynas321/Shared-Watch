import CreateRoomForm from "./CreateRoomForm";
import Button from "../shared/Button";
import { useState } from "react";
import { createPortal } from "react-dom";

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
  const [isOpen, setIsOpen] = useState(false);

  return (
    <>
      <Button
        text="Create Room"
        classNames="btn btn-success"
        disabled={!isEnabled}
        onClick={() => setIsOpen(true)}
      />

      {isOpen && createPortal(
        <>
          <div className="modal fade show d-block" tabIndex={-1} role="dialog" aria-modal="true">
            <div className="modal-dialog" role="document" style={{ marginTop: "3.75rem" }}>
              <div className="modal-content">
                <CreateRoomForm
                  acceptText={acceptText}
                  declineText={declineText}
                  onClose={() => setIsOpen(false)}
                />
              </div>
            </div>
          </div>
          <div className="modal-backdrop fade show" onClick={() => setIsOpen(false)} />
        </>,
        document.body
      )}
    </>
  );
}

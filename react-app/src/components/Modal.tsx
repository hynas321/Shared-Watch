import { ReactNode } from "react";
import Button, { ButtonProps } from "./Button"

export interface ModalProps {
  title: string;
  acceptText: string;
  declineText: string;
  buttonProps: ButtonProps;
  body: ReactNode;
  onAcceptClick: () => void;
}

export default function Modal({title, acceptText, declineText, buttonProps, body, onAcceptClick}: ModalProps) {
  return (
    <>
      <div>
        <span
          data-bs-toggle="modal"
          data-bs-target="#exampleModal"
        >
          <Button
            text={buttonProps.text}
            classNames={buttonProps.classNames}
            styles={buttonProps.styles}
            onClick={buttonProps.onClick}
          />
        </span>
      </div>
      <div className="modal fade" id="exampleModal" tabIndex={-1} role="dialog">
        <div className="modal-dialog" role="document">
          <div className="modal-content">
            <div className="modal-header bg-light">
              <h5 className="modal-title">{title}</h5>
              <button
                type="button"
                className="btn-close"
                data-bs-dismiss="modal"
                aria-label="Close"
              ></button>
            </div>
            <div className="modal-body bg-light">
              {body}
            </div>
            <div className="modal-footer bg-light">
              <button
                type="button"
                className="btn btn-primary"
                data-bs-dismiss="modal"
                onClick={onAcceptClick}
              >
                {acceptText}
              </button>
              <button
                type="button"
                className="btn btn-secondary"
                data-bs-dismiss="modal"
              >
                {declineText}
              </button>
            </div>
          </div>
        </div>
      </div>
    </>
  )
}
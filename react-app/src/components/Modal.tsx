export interface ModalProps {
  isDisplayed: boolean;
}

export default function Modal({isDisplayed}: ModalProps) {
  return (
    <div
      className="modal fade"
      id="my-modal"
      data-bs-backdrop="static"
      data-bs-keyboard="false"
      tabIndex={-1}
      aria-labelledby="staticBackdropLiveLabel"
      aria-hidden="true"
      style={{display: isDisplayed ? "block" : "none"}}
    >
      <div className="modal-dialog">
        <div className="modal-content">
          <div className="modal-header">
            <h1 className="modal-title fs-5"id="staticBackdropLiveLabel">Modal title</h1>
            <button type="button" className="btn-close" data-bs-dismiss="modal" aria-label="Close">
            </button>
          </div>
          <div className="modal-body">
            <input/>
            <p>I will not close if you click outside of me. Don't even try to press escape key.</p>
          </div>
          <div className="modal-footer">
            <button type="button" className="btn btn-secondary" data-bs-dismiss="modal">Close</button>
            <button type="button" className="btn btn-primary">Understood</button>
          </div>
        </div>
      </div>
    </div>
  )
}

import Button from "./Button";

export default function Friends() {
  return (
    <div>
      <div className="d-flex">
        <Button text={"List"}
          classNames={"btn btn-secondary btn-rectangular"}
          onClick={() => {}} 
        />
        <Button text={"Add"}
          classNames={"btn btn-secondary btn-rectangular"}
          onClick={() => {}} 
        />
        <Button text={"Invitations"}
          classNames={"btn btn-secondary btn-rectangular"}
          onClick={() => {}} 
        />
      </div>
      <div style={{height: "300px"}}>
        
      </div>
    </div>
  )
}

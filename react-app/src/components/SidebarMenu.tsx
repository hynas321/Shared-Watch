import { SidebarElement } from "../types/SidebarElement"

export default function SidebarMenu() {
  const styles = {
    cursor: "pointer"
  };

  const element: SidebarElement = {
    image: undefined,
    text: "Test"
  };

  const elements: SidebarElement[] = [
    element,
    element,
    element,
    element,
    element
  ];
  
  return (
    <ul className="list-group rounded-4">
      {
        elements.map((item, index) => (
          <li 
            key={index}
            className="list-group-item color-bg-muted border-2"
            style={styles}
          >
            <h5 className="d-inline">{item.image}</h5>
            <h6>{item.text}</h6>
          </li>
        ))
      }
    </ul>
  )
}
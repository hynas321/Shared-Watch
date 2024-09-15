import React from "react";
import { InputField } from "./shared/InputField";
import Switch from "./shared/Switch";
import CreateRoomModal from "./CreateRoomModal";

interface RoomFiltersProps {
  searchText: string;
  onSearchTextChange: (value: string) => void;
  displayOnlyAvailableRooms: boolean;
  onDisplayOnlyAvailableRoomsChange: (value: boolean) => void;
}

const RoomFilters: React.FC<RoomFiltersProps> = ({
  searchText,
  onSearchTextChange,
  displayOnlyAvailableRooms,
  onDisplayOnlyAvailableRoomsChange,
}) => {
  return (
    <div className="mt-4">
      <div className="row d-flex justify-content-between align-items-center text-center">
        <div className="col-6">
          <InputField
            classNames="form-control rounded-3"
            placeholder="Search room name"
            value={searchText}
            trim={false}
            isEnabled={true}
            maxCharacters={60}
            onChange={onSearchTextChange}
          />
        </div>
        <div className="col-6 text-end">
          <CreateRoomModal acceptText="Create" declineText="Go back" />
        </div>
      </div>
      <div className="mt-3 mb-3">
        <Switch
          label="Show only available rooms"
          isChecked={displayOnlyAvailableRooms}
          isEnabled={true}
          onCheckChange={onDisplayOnlyAvailableRoomsChange}
        />
      </div>
    </div>
  );
};

export default RoomFilters;

export class HttpApiEndpoints {
  static getAllRooms = "api/Room/GetAll";
  static getRoom = "api/room/Get";
  static createRoom = "api/Room/Create";
  static roomExists = "api/Room/Exists";
  static joinRoom = "api/User/JoinRoom";
  static leaveRoom = "api/User/Leave";

  static appHubConnection = "/Hub/Room";
}
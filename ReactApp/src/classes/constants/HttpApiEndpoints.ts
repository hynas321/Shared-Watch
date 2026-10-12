export class HttpApiEndpoints {
  static getAllRooms = "api/Room/GetAll";
  static health = "api/Room/Health";
  static getRoom = "api/room/Get";
  static createRoom = "api/Room/Create";
  static roomExists = "api/Room/Exists";
  static joinRoom = "api/User/Join";
  static refreshToken = "api/User/RefreshToken";
  static leaveRoom = "api/User/Leave";

  static appHubConnection = "/Hub/Room";
}

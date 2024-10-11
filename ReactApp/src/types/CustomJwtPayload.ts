interface CustomJwtPayload {
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"?: string;
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"?: "Admin" | "User";
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/hash"?: string;
  exp?: number;
  iss?: string;
  aud?: string;
}

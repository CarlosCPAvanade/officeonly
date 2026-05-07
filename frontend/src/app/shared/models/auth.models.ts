export interface LoginRequest {
  userName: string;
  password: string;
}

export interface AuthUser {
  id: string;
  userName: string;
  email: string;
  role: string;
}

export interface LoginResponse {
  token: string;
  expiresAtUtc: string;
  user: AuthUser;
}

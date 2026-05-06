export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  expiresAt: string;
  user: AuthUser;
}

export interface AuthUser {
  id: number;
  email: string;
  name: string;
  role?: string;
  subscriptionId: number;
}

export interface JwtPayload {
  userId?: string;
  email?: string;
  role?: string;
  subscriptionId?: string;
  exp?: number;
  iat?: number;
  iss?: string;
  aud?: string;
  [key: string]: unknown;
}

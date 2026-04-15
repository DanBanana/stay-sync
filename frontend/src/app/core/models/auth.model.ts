export type UserRole = 'SuperAdmin' | 'PropertyManager';

export interface AuthUser {
  id: string;
  email: string;
  role: UserRole;
  propertyManagerId: string | null;
}

export interface AuthResult {
  token: string;
  expiresAt: string;
  role: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

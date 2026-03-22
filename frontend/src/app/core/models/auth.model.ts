export interface AuthUser {
  id: string;
  email: string;
  role: 'SuperAdmin' | 'PropertyManager';
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

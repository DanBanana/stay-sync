export interface InviteToken {
  id: string;
  email: string;
  role: string;
  expiresAt: string;
  usedAt: string | null;
  isPending: boolean;
}

export interface CreateInviteResult {
  token: string;
  expiresAt: string;
}

export interface InviteInfo {
  email: string;
  role: string;
}

export interface AcceptInviteRequest {
  token: string;
  name: string;
  password: string;
}

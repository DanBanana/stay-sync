import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AcceptInviteRequest, CreateInviteResult, InviteInfo, InviteToken } from '../models/invite.model';
import { UserRole } from '../models/auth.model';

@Injectable({ providedIn: 'root' })
export class InviteService {
  private readonly base = `${environment.apiBaseUrl}/api/invites`;

  constructor(private http: HttpClient) {}

  createInvite(email: string, role: UserRole): Observable<CreateInviteResult> {
    return this.http.post<CreateInviteResult>(this.base, { email, role });
  }

  getInvites(): Observable<InviteToken[]> {
    return this.http.get<InviteToken[]>(this.base);
  }

  revokeInvite(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  validateToken(token: string): Observable<InviteInfo> {
    return this.http.get<InviteInfo>(`${this.base}/validate`, { params: { token } });
  }

  acceptInvite(request: AcceptInviteRequest): Observable<void> {
    return this.http.post<void>(`${this.base}/accept`, request);
  }
}

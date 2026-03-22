import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResult, LoginRequest } from '../models/auth.model';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly baseUrl = `${environment.apiBaseUrl}/api/auth`;

  constructor(private http: HttpClient) {}

  login(request: LoginRequest): Observable<AuthResult> {
    return this.http.post<AuthResult>(`${this.baseUrl}/login`, request);
  }
}

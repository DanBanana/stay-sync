import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ManagedUser } from '../models/user-management.model';

@Injectable({ providedIn: 'root' })
export class UserManagementService {
  private readonly base = `${environment.apiBaseUrl}/api/users`;

  constructor(private http: HttpClient) {}

  getUsers(): Observable<ManagedUser[]> {
    return this.http.get<ManagedUser[]>(this.base);
  }

  deactivateUser(id: string): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}/deactivate`, {});
  }

  activateUser(id: string): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}/activate`, {});
  }

  deleteUser(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}

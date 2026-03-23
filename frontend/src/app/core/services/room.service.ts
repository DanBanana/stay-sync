import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Room } from '../models/room.model';

@Injectable({ providedIn: 'root' })
export class RoomService {
  private readonly base = `${environment.apiBaseUrl}/api/rooms`;

  constructor(private http: HttpClient) {}

  getByProperty(propertyId: string): Observable<Room[]> {
    return this.http.get<Room[]>(`${this.base}/by-property/${propertyId}`);
  }

  create(propertyId: string, name: string): Observable<Room> {
    return this.http.post<Room>(this.base, { propertyId, name });
  }

  update(id: string, name: string): Observable<Room> {
    return this.http.put<Room>(`${this.base}/${id}`, { id, name });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}

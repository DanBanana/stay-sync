import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ExternalCalendar, SyncCalendarResult } from '../models/external-calendar.model';

@Injectable({ providedIn: 'root' })
export class ExternalCalendarService {
  private readonly base = `${environment.apiBaseUrl}/api/external-calendars`;

  constructor(private http: HttpClient) {}

  getByRoom(roomId: string): Observable<ExternalCalendar[]> {
    return this.http.get<ExternalCalendar[]>(`${this.base}/by-room/${roomId}`);
  }

  create(roomId: string, platform: string, icsUrl: string): Observable<ExternalCalendar> {
    return this.http.post<ExternalCalendar>(this.base, { roomId, platform, icsUrl });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }

  sync(id: string): Observable<SyncCalendarResult> {
    return this.http.post<SyncCalendarResult>(`${this.base}/${id}/sync`, {});
  }
}

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Booking, CalendarBooking, CreateManualBookingRequest, UpdateManualBookingRequest } from '../models/booking.model';

@Injectable({ providedIn: 'root' })
export class BookingService {
  private readonly base = `${environment.apiBaseUrl}/api/bookings`;

  constructor(private http: HttpClient) {}

  getByRoom(roomId: string): Observable<Booking[]> {
    return this.http.get<Booking[]>(`${this.base}/by-room/${roomId}`);
  }

  getByProperty(propertyId: string): Observable<Booking[]> {
    return this.http.get<Booking[]>(`${this.base}/by-property/${propertyId}`);
  }

  getForCalendar(propertyId: string, from: string, to: string): Observable<CalendarBooking[]> {
    return this.http.get<CalendarBooking[]>(`${this.base}/calendar`, {
      params: { propertyId, from, to }
    });
  }

  createManual(req: CreateManualBookingRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.base, req);
  }

  updateManual(id: string, req: UpdateManualBookingRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}`, req);
  }

  deleteManual(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Booking } from '../models/booking.model';

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
}

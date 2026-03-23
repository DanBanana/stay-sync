import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PropertyManager } from '../models/property-manager.model';

@Injectable({ providedIn: 'root' })
export class PropertyManagerService {
  private readonly base = `${environment.apiBaseUrl}/api/property-managers`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<PropertyManager[]> {
    return this.http.get<PropertyManager[]>(this.base);
  }
}

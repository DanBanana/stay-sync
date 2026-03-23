import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Property } from '../models/property.model';

@Injectable({ providedIn: 'root' })
export class PropertyService {
  private readonly base = `${environment.apiBaseUrl}/api/properties`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Property[]> {
    return this.http.get<Property[]>(this.base);
  }

  getById(id: string): Observable<Property> {
    return this.http.get<Property>(`${this.base}/${id}`);
  }

  create(name: string, address?: string, propertyManagerId?: string): Observable<Property> {
    return this.http.post<Property>(this.base, { name, address, propertyManagerId });
  }

  update(id: string, name: string, address?: string): Observable<Property> {
    return this.http.put<Property>(`${this.base}/${id}`, { name, address });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}

import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root',
})
export class ApiService {
  constructor() {}
  private http = inject(HttpClient);

  private baseUrl = 'https://localhost:7168/api';
  get<T>(url: string) {
    return this.http.get<T>(`${this.baseUrl}/${url}`);
  }

  post<T>(url: string, body: unknown) {
    return this.http.post<T>(`${this.baseUrl}/${url}`, body);
  }
}

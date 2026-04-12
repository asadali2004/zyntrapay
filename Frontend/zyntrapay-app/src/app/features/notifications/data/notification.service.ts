import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Notification } from '../models/notification.models';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private apiUrl = `${environment.apiUrl}/notification`;

  constructor(private http: HttpClient) {}

  getAllNotifications(): Observable<Notification[]> {
    return this.http.get<Notification[]>(this.apiUrl);
  }

  markAsRead(id: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/read`, {});
  }
}

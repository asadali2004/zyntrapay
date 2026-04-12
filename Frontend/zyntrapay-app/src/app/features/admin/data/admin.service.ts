import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { KycRequest, ReviewKycRequest, DashboardStats, AdminAuditAction, AdminUserDetails } from '../models/admin.models';
import { User } from '../../auth/models/auth.models';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private apiUrl = `${environment.apiUrl}/admin`;

  constructor(private http: HttpClient) {}

  getPendingKycs(): Observable<KycRequest[]> {
    return this.http.get<KycRequest[]>(`${this.apiUrl}/kyc/pending`);
  }

  reviewKyc(kycId: number, data: ReviewKycRequest): Observable<any> {
    return this.http.put(`${this.apiUrl}/kyc/${kycId}/review`, data);
  }

  getAllUsers(): Observable<User[]> {
    return this.http.get<User[]>(`${this.apiUrl}/users`);
  }

  toggleUserStatus(userId: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/users/${userId}/toggle`, {});
  }

  getDashboard(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${this.apiUrl}/dashboard`);
  }

  getRecentActions(take = 10): Observable<AdminAuditAction[]> {
    return this.http.get<AdminAuditAction[]>(`${this.apiUrl}/actions/recent`, {
      params: { take }
    });
  }

  getUserDetails(authUserId: number): Observable<AdminUserDetails> {
    return this.http.get<AdminUserDetails>(`${this.apiUrl}/users/${authUserId}/details`);
  }
}

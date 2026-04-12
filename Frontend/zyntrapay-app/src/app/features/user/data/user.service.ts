import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { CreateProfileRequest, UserProfile, SubmitKycRequest, KycStatus, UserIdentity } from '../models/user.models';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private apiUrl = `${environment.apiUrl}/user`;

  constructor(private http: HttpClient) {}

  createProfile(data: CreateProfileRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/profile`, data);
  }

  getProfile(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.apiUrl}/profile`);
  }

  getIdentityByAuthUserId(authUserId: number): Observable<UserIdentity> {
    return this.http.get<UserIdentity>(`${this.apiUrl}/identity/${authUserId}`);
  }

  submitKyc(data: SubmitKycRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/kyc`, data);
  }

  getKycStatus(): Observable<KycStatus> {
    return this.http.get<KycStatus>(`${this.apiUrl}/kyc`);
  }
}

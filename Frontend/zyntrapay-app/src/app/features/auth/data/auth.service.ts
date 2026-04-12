import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, tap } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  RegisterRequest,
  AdminRegisterRequest,
  LoginRequest,
  LoginResponse,
  GoogleLoginRequest,
  UpdatePhoneRequest,
  SendOtpRequest,
  VerifyOtpRequest,
  ForgotPasswordRequest,
  ResetPasswordRequest,
  RefreshTokenRequest,
  User,
  RecipientLookup
} from '../models/auth.models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/auth`;
  private currentUserSubject = new BehaviorSubject<LoginResponse | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    const storedUser = localStorage.getItem('currentUser');
    if (storedUser) {
      this.currentUserSubject.next(JSON.parse(storedUser));
    }
  }

  register(data: RegisterRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, data);
  }

  registerAdmin(data: AdminRegisterRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/register-admin`, data);
  }

  login(data: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, data).pipe(
      tap(response => this.persistAuthState(response))
    );
  }

  googleLogin(data: GoogleLoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/google-login`, data).pipe(
      tap(response => this.persistAuthState(response))
    );
  }

  updatePhone(data: UpdatePhoneRequest): Observable<any> {
    return this.http.put(`${this.apiUrl}/update-phone`, data).pipe(
      tap(() => this.markPhoneUpdateCompleted())
    );
  }

  requestRegistrationOtp(data: SendOtpRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/register/request-otp`, data);
  }

  verifyRegistrationOtp(data: VerifyOtpRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/register/verify-otp`, data);
  }

  forgotPassword(data: ForgotPasswordRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/forgot-password`, data);
  }

  resetPassword(data: ResetPasswordRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/reset-password`, data);
  }

  lookupUserByEmail(email: string): Observable<RecipientLookup> {
    return this.http.get<RecipientLookup>(`${this.apiUrl}/users/lookup`, {
      params: { email }
    });
  }

  refreshToken(data: RefreshTokenRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/refresh-token`, data).pipe(
      tap(response => this.persistAuthState(response))
    );
  }

  logout(): void {
    localStorage.removeItem('currentUser');
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    this.currentUserSubject.next(null);
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  getRefreshToken(): string | null {
    return localStorage.getItem('refreshToken');
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  getCurrentUser(): LoginResponse | null {
    return this.currentUserSubject.value;
  }

  isAdmin(): boolean {
    const user = this.getCurrentUser();
    return user?.role === 'Admin';
  }

  private persistAuthState(response: LoginResponse): void {
    localStorage.setItem('currentUser', JSON.stringify(response));
    localStorage.setItem('token', response.token);
    localStorage.setItem('refreshToken', response.refreshToken);
    this.currentUserSubject.next(response);
  }

  private markPhoneUpdateCompleted(): void {
    const current = this.currentUserSubject.value;
    if (!current) {
      return;
    }

    const updatedUser: LoginResponse = {
      ...current,
      phoneUpdateRequired: false
    };

    localStorage.setItem('currentUser', JSON.stringify(updatedUser));
    this.currentUserSubject.next(updatedUser);
  }
}

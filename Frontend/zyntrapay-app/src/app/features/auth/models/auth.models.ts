export interface RegisterRequest {
  email: string;
  password: string;
  fullName: string;
  phoneNumber?: string;
}

export interface AdminRegisterRequest {
  email: string;
  password: string;
  fullName: string;
  phoneNumber?: string;
  adminSecretKey: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  email: string;
  role: string;
  fullName?: string;
  phoneUpdateRequired: boolean;
}

export interface GoogleLoginRequest {
  idToken: string;
}

export interface UpdatePhoneRequest {
  phoneNumber: string;
}

export interface SendOtpRequest {
  email: string;
}

export interface VerifyOtpRequest {
  email: string;
  otp: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  email: string;
  otp: string;
  newPassword: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface User {
  id: number;
  email: string;
  fullName: string;
  phoneNumber?: string;
  role: string;
  isActive: boolean;
  createdAt: Date;
}

export interface RecipientLookup {
  id: number;
  email: string;
  fullName?: string;
  phoneNumber: string;
  role: string;
  isActive: boolean;
  createdAt: string;
}

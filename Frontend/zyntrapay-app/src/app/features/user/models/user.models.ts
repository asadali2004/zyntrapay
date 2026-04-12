export interface CreateProfileRequest {
  fullName: string;
  dateOfBirth: string;
  address: string;
  city: string;
  state: string;
  pinCode: string;
}

export interface UserProfile {
  id: number;
  authUserId: number;
  fullName: string;
  dateOfBirth: string;
  address: string;
  city: string;
  state: string;
  pinCode: string;
}

export interface UserIdentity {
  authUserId: number;
  fullName: string;
}

export interface SubmitKycRequest {
  documentType: string;
  documentNumber: string;
}

export interface KycStatus {
  id: number;
  documentType?: string;
  documentNumber?: string;
  status: 'Pending' | 'Approved' | 'Rejected';
  rejectionReason?: string;
  submittedAt: string;
  reviewedAt?: string;
}

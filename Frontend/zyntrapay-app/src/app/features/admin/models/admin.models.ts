export interface KycRequest {
  id: number;
  authUserId: number;
  fullName: string;
  documentType: string;
  documentNumber: string;
  documentUrl: string;
  status: string;
  submittedAt: Date;
}

export interface ReviewKycRequest {
  status: string;
  rejectionReason?: string;
  targetAuthUserId?: number;
  userEmail?: string;
}

export interface DashboardStats {
  totalUsers: number;
  activeUsers: number;
  pendingKyc: number;
  approvedKyc: number;
  rejectedKyc: number;
}

export interface AdminAction {
  id: number;
  adminAuthUserId: number;
  actionType: string;
  targetUserId?: number;
  description: string;
  performedAt: Date;
}

export interface AdminAuditAction {
  id: number;
  adminAuthUserId: number;
  actionType: string;
  targetUserId: number;
  remarks?: string | null;
  performedAt: string;
}

export interface AdminUserProfile {
  id: number;
  authUserId: number;
  fullName: string;
  dateOfBirth: string;
  address: string;
  city: string;
  state: string;
  pinCode: string;
}

export interface AdminUserDetails {
  profile: AdminUserProfile | null;
  kyc: KycRequest | null;
}

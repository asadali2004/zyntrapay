import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { AuthService } from '../../../auth/data/auth.service';
import { AdminService } from '../../data/admin.service';
import { AdminAuditAction, AdminUserDetails, DashboardStats, KycRequest } from '../../models/admin.models';
import { User } from '../../../auth/models/auth.models';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.scss'
})
export class AdminDashboardComponent implements OnInit {
  activeTab = signal<'overview' | 'kyc' | 'users'>('overview');
  selectedKyc = signal<KycRequest | null>(null);
  selectedUser = signal<User | null>(null);
  selectedUserDetails = signal<AdminUserDetails | null>(null);
  stats: DashboardStats | null = null;
  pendingKycs: KycRequest[] = [];
  users: User[] = [];
  recentActions: AdminAuditAction[] = [];
  loading = true;
  detailLoading = signal(false);
  reviewSubmitting = signal(false);
  statusSubmittingUserId = signal<number | null>(null);

  readonly rejectionForm;

  constructor(
    private authService: AuthService,
    private adminService: AdminService,
    private fb: FormBuilder,
    private router: Router
  ) {
    this.rejectionForm = this.fb.group({
      rejectionReason: ['', [Validators.required, Validators.minLength(5)]]
    });
  }

  ngOnInit(): void {
    if (!this.authService.isAdmin()) {
      this.router.navigate(['/dashboard']);
      return;
    }

    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.loading = true;

    this.adminService.getDashboard().subscribe({
      next: (stats) => {
        this.stats = stats;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading dashboard:', error);
        this.loading = false;
      }
    });

    this.adminService.getPendingKycs().subscribe({
      next: (kycs) => {
        this.pendingKycs = [...kycs]
          .sort((a, b) => new Date(b.submittedAt).getTime() - new Date(a.submittedAt).getTime());
      },
      error: (error) => console.error('Error loading KYCs:', error)
    });

    this.adminService.getAllUsers().subscribe({
      next: (users) => {
        this.users = [...users]
          .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
      },
      error: (error) => console.error('Error loading users:', error)
    });

    this.adminService.getRecentActions(8).subscribe({
      next: (actions) => {
        this.recentActions = actions;
      },
      error: (error) => console.error('Error loading admin actions:', error)
    });
  }

  openKycReview(kyc: KycRequest): void {
    this.selectedKyc.set(kyc);
    this.rejectionForm.reset();
  }

  closeKycReview(): void {
    this.selectedKyc.set(null);
    this.rejectionForm.reset();
  }

  openUserDetails(user: User): void {
    this.selectedUser.set(user);
    this.selectedUserDetails.set(null);
    this.detailLoading.set(true);

    this.adminService.getUserDetails(user.id).subscribe({
      next: (details) => {
        this.selectedUserDetails.set(details);
        this.detailLoading.set(false);
      },
      error: () => {
        this.selectedUserDetails.set(null);
        this.detailLoading.set(false);
      }
    });
  }

  closeUserDetails(): void {
    this.selectedUser.set(null);
    this.selectedUserDetails.set(null);
  }

  approveKyc(kycId: number): void {
    this.reviewSubmitting.set(true);
    this.adminService.reviewKyc(kycId, { status: 'Approved' }).subscribe({
      next: () => {
        this.reviewSubmitting.set(false);
        this.closeKycReview();
        this.loadDashboardData();
      },
      error: (error) => {
        console.error('Error approving KYC:', error);
        this.reviewSubmitting.set(false);
      }
    });
  }

  rejectKyc(kycId: number): void {
    if (this.rejectionForm.invalid) {
      this.rejectionForm.markAllAsTouched();
      return;
    }

    this.reviewSubmitting.set(true);
    this.adminService.reviewKyc(kycId, {
      status: 'Rejected',
      rejectionReason: this.rejectionForm.value.rejectionReason ?? 'Documents not clear'
    }).subscribe({
      next: () => {
        this.reviewSubmitting.set(false);
        this.closeKycReview();
        this.loadDashboardData();
      },
      error: (error) => {
        console.error('Error rejecting KYC:', error);
        this.reviewSubmitting.set(false);
      }
    });
  }

  toggleUserStatus(userId: number): void {
    this.statusSubmittingUserId.set(userId);
    this.adminService.toggleUserStatus(userId).subscribe({
      next: () => {
        this.statusSubmittingUserId.set(null);
        if (this.selectedUser()?.id === userId) {
          const current = this.selectedUser();
          if (current) {
            this.selectedUser.set({ ...current, isActive: !current.isActive });
          }
        }
        this.loadDashboardData();
      },
      error: (error) => {
        console.error('Error toggling user status:', error);
        this.statusSubmittingUserId.set(null);
      }
    });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  formatJoinedDate(value: Date): string {
    const date = this.parseBackendDate(value);
    if (!date) {
      return 'Not available';
    }

    return new Intl.DateTimeFormat('en-IN', {
      dateStyle: 'medium',
      timeZone: 'Asia/Kolkata'
    }).format(date);
  }

  formatSubmittedAt(value: Date): string {
    const date = this.parseBackendDate(value);
    if (!date) {
      return 'Not available';
    }

    return new Intl.DateTimeFormat('en-IN', {
      dateStyle: 'medium',
      timeStyle: 'short',
      timeZone: 'Asia/Kolkata'
    }).format(date);
  }

  maskDocumentNumber(value: string): string {
    if (!value) {
      return 'Not available';
    }

    const tail = value.slice(-4);
    return `${'*'.repeat(Math.max(0, value.length - 4))}${tail}`;
  }

  formatAuditTime(value: string): string {
    const date = this.parseBackendDate(value);
    if (!date) {
      return 'Not available';
    }

    return new Intl.DateTimeFormat('en-IN', {
      dateStyle: 'medium',
      timeStyle: 'short',
      timeZone: 'Asia/Kolkata'
    }).format(date);
  }

  formatActionLabel(actionType: string): string {
    return actionType
      .split('_')
      .filter(Boolean)
      .map(part => part.charAt(0) + part.slice(1).toLowerCase())
      .join(' ');
  }

  get rejectionReasonError(): string {
    const control = this.rejectionForm.get('rejectionReason');
    if (!control?.touched || !control.invalid) {
      return '';
    }

    if (control.hasError('required')) {
      return 'A rejection reason is required.';
    }

    return 'Please enter at least 5 characters.';
  }

  private parseBackendDate(value: string | Date | null | undefined): Date | null {
    if (!value) {
      return null;
    }

    if (value instanceof Date) {
      return isNaN(value.getTime()) ? null : value;
    }

    const trimmed = value.trim();
    if (!trimmed) {
      return null;
    }

    const hasTimezone = /([zZ]|[+-]\d{2}:?\d{2})$/.test(trimmed);
    const normalized = hasTimezone ? trimmed : `${trimmed}Z`;
    const parsed = new Date(normalized);

    return isNaN(parsed.getTime()) ? null : parsed;
  }
}

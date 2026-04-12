import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../auth/data/auth.service';
import { WalletService } from '../../../wallet/data/wallet.service';
import { UserService } from '../../../user/data/user.service';
import { RewardsService } from '../../../rewards/data/rewards.service';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { LoginResponse, RecipientLookup } from '../../../auth/models/auth.models';
import { WalletBalance, Transaction } from '../../../wallet/models/wallet.models';
import { RewardsSummary } from '../../../rewards/models/rewards.models';
import { KycStatus, UserProfile } from '../../../user/models/user.models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  currentUser: LoginResponse | null = null;
  todayDate = this.getCurrentDateIst();
  activeTab = signal<'overview' | 'wallet' | 'transactions' | 'rewards' | 'settings'>('overview');

  // State signals
  hasProfile = signal(true);
  hasKyc = signal(false);
  hasWallet = signal(true);

  profileLoading = signal(true);
  kycLoading = signal(true);
  walletLoading = signal(true);
  txLoading = signal(true);
  rewardsLoading = signal(true);

  profile = signal<UserProfile | null>(null);
  kyc = signal<KycStatus | null>(null);
  wallet = signal<WalletBalance | null>(null);
  transactions = signal<Transaction[]>([]);
  rewards = signal<RewardsSummary | null>(null);

  // Forms
  profileForm: FormGroup;
  profileSubmitting = signal(false);

  walletSubmitting = signal(false);

  sendMoneyOpen = signal(false);
  sendMoneyForm: FormGroup;
  sendLoading = signal(false);
  recipientPreview = signal<RecipientLookup | null>(null);
  recipientLookupLoading = signal(false);
  recipientLookupError = signal('');
  recipientLookupVerified = signal(false);

  topUpOpen = signal(false);
  topUpForm: FormGroup;
  topUpLoading = signal(false);

  kycOpen = signal(false);
  kycForm: FormGroup;
  kycSubmitting = signal(false);
  passwordResetSending = signal(false);
  resetPasswordOpen = signal(false);
  resetPasswordSubmitting = signal(false);
  resetPasswordForm: FormGroup;

  constructor(
    private authService: AuthService,
    private userService: UserService,
    private walletService: WalletService,
    private rewardsService: RewardsService,
    private toastService: ToastService,
    private fb: FormBuilder,
    private router: Router
  ) {
    this.profileForm = this.fb.group({
      fullName: ['', [Validators.required, Validators.minLength(3)]],
      dateOfBirth: ['', [Validators.required, this.noFutureDateValidator()]],
      address: ['', [Validators.required]],
      city: ['', [Validators.required]],
      state: ['', [Validators.required]],
      pinCode: ['', [Validators.required, Validators.pattern('^\\d{6}$')]]
    });

    this.sendMoneyForm = this.fb.group({
      receiverEmail: ['', [Validators.required, Validators.email]],
      amount: ['', [Validators.required, Validators.min(0.01)]],
      description: ['']
    });

    this.topUpForm = this.fb.group({
      paymentMethod: ['UPI', Validators.required],
      amount: ['', [Validators.required, Validators.min(1), Validators.max(50000)]]
    });

    this.kycForm = this.fb.group({
      documentType: ['Aadhaar', [Validators.required]],
      documentNumber: ['', [Validators.required, Validators.minLength(8)]]
    });

    this.resetPasswordForm = this.fb.group({
      otp: ['', [Validators.required, Validators.pattern('^\\d{6}$')]],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordsMatchValidator() });
  }

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();
    if (!this.currentUser) {
      this.router.navigate(['/login']);
      return;
    }
    
    // Workflow: Load sequence
    this.loadProfile();
  }

  loadProfile(): void {
    this.profileLoading.set(true);
    this.userService.getProfile().subscribe({
      next: (p) => {
        this.profile.set(p);
        this.hasProfile.set(true);
        this.profileForm.patchValue({
          fullName: p.fullName,
          dateOfBirth: this.toDateInputValue(p.dateOfBirth),
          address: p.address,
          city: p.city,
          state: p.state,
          pinCode: p.pinCode
        });
        this.profileLoading.set(false);
        this.loadKycStatus();
      },
      error: (err) => {
        this.profileLoading.set(false);
        if (err.status === 404) {
          this.hasProfile.set(false);
          this.hasKyc.set(false);
          this.hasWallet.set(false);
        }
      }
    });
  }

  submitProfile(): void {
    this.normalizeProfileInputs();

    if (this.profileForm.invalid) {
      this.profileForm.markAllAsTouched();
      return;
    }
    this.profileSubmitting.set(true);
    this.userService.createProfile(this.profileForm.value).subscribe({
      next: () => {
        this.profileSubmitting.set(false);
        this.toastService.success('Profile Created', 'Your user profile has been successfully generated.');
        this.loadProfile();
      },
      error: (err) => {
        this.profileSubmitting.set(false);
        this.toastService.error('Profile Creation Failed', err.error?.message || 'Unknown error');
      }
    });
  }

  loadKycStatus(): void {
    this.kycLoading.set(true);
    this.userService.getKycStatus().subscribe({
      next: (status) => {
        this.kyc.set(status);
        this.hasKyc.set(true);
        this.kycLoading.set(false);
        this.loadWallet();
      },
      error: (err) => {
        this.kycLoading.set(false);
        if (err.status === 404) {
          this.hasKyc.set(false);
          // Still attempt to load wallet — KYC is not a hard prerequisite for wallet per API contract
          this.loadWallet();
        }
      }
    });
  }

  submitKyc(): void {
    this.kycForm.patchValue({
      documentNumber: (this.kycForm.value.documentNumber || '').trim()
    });

    if (this.kycForm.invalid) {
      this.kycForm.markAllAsTouched();
      return;
    }

    this.kycSubmitting.set(true);
    this.userService.submitKyc(this.kycForm.getRawValue()).subscribe({
      next: () => {
        this.kycSubmitting.set(false);
        this.kycOpen.set(false);
        this.toastService.success('KYC Submitted', 'Your verification request is now under review.');
        this.loadKycStatus();
      },
      error: (err) => {
        this.kycSubmitting.set(false);
        this.toastService.error('KYC Failed', err.error?.message || 'Could not submit KYC details.');
      }
    });
  }

  loadWallet(): void {
    this.walletLoading.set(true);
    this.walletService.getBalance().subscribe({
      next: (b) => {
        this.wallet.set(b);
        this.hasWallet.set(true);
        this.walletLoading.set(false);
        this.loadTransactions();
        this.loadRewards();
      },
      error: (err) => {
        this.walletLoading.set(false);
        if (err.status === 404 || err.status === 400) {
          this.hasWallet.set(false);
        }
      }
    });
  }

  createWallet(): void {
    this.walletSubmitting.set(true);
    this.walletService.createWallet().subscribe({
      next: () => {
        this.walletSubmitting.set(false);
        this.toastService.success('Wallet Created', 'Your virtual wallet is ready to use.');
        this.loadWallet();
      },
      error: (err) => {
        this.walletSubmitting.set(false);
        const message = this.extractErrorMessage(err, 'Could not create wallet.');
        const code = err?.error?.errorCode as string | undefined;

        if (code === 'WALLET_ALREADY_EXISTS' || message.toLowerCase().includes('wallet already exists')) {
          this.toastService.info('Wallet Ready', 'Your wallet already exists. Refreshing your account view now.');
          this.loadWallet();
          return;
        }

        this.toastService.error('Wallet Error', message);
      }
    });
  }

  loadTransactions(): void {
    this.txLoading.set(true);
    this.walletService.getTransactions().subscribe({
      next: (t) => { this.transactions.set(t); this.txLoading.set(false); },
      error: () => { this.txLoading.set(false); }
    });
  }

  loadRewards(): void {
    this.rewardsLoading.set(true);
    this.rewardsService.getSummary().subscribe({
      next: (r) => { this.rewards.set(r); this.rewardsLoading.set(false); },
      error: () => { this.rewardsLoading.set(false); }
    });
  }

  submitSendMoney(): void {
    if (this.sendMoneyForm.invalid) {
      this.sendMoneyForm.markAllAsTouched();
      return;
    }

    if (!this.recipientLookupVerified() || !this.recipientPreview()) {
      this.toastService.error('Verify Recipient', 'Please verify the recipient before sending money.');
      return;
    }

    this.sendLoading.set(true);
    const recipientEmail = this.sendMoneyForm.value.receiverEmail;
    
    const payload = {
      receiverEmail: recipientEmail,
      amount: parseFloat(this.sendMoneyForm.value.amount),
      description: this.sendMoneyForm.value.description || 'Fund Transfer'
    };

    this.walletService.sendMoney(payload).subscribe({
      next: () => {
        this.sendLoading.set(false);
        this.sendMoneyOpen.set(false);
        this.sendMoneyForm.reset();
        this.recipientPreview.set(null);
        this.recipientLookupVerified.set(false);
        this.recipientLookupError.set('');
        this.toastService.success('Money Sent!', `Transfer to ${recipientEmail} was successful.`);
        this.loadWallet(); // Refresh balance & txs
      },
      error: (err) => {
        this.sendLoading.set(false);
        this.toastService.error('Transfer Failed', this.extractErrorMessage(err, 'An unexpected error occurred.'));
      }
    });
  }

  openSendMoneyModal(): void {
    this.sendMoneyForm.reset();
    this.recipientPreview.set(null);
    this.recipientLookupVerified.set(false);
    this.recipientLookupError.set('');
    this.sendMoneyOpen.set(true);
  }

  verifyRecipient(): void {
    const emailControl = this.sendMoneyForm.get('receiverEmail');
    if (emailControl?.invalid) {
      emailControl.markAsTouched();
      this.recipientLookupVerified.set(false);
      this.recipientPreview.set(null);
      this.recipientLookupError.set('Enter a valid recipient email first.');
      return;
    }

    const email = (emailControl?.value || '').trim().toLowerCase();
    if (!email) {
      this.recipientLookupVerified.set(false);
      this.recipientPreview.set(null);
      this.recipientLookupError.set('Enter a recipient email first.');
      return;
    }

    if (email === this.currentUser?.email?.toLowerCase()) {
      this.recipientLookupVerified.set(false);
      this.recipientPreview.set(null);
      this.recipientLookupError.set('You cannot transfer to your own account.');
      return;
    }

    this.recipientLookupLoading.set(true);
    this.recipientLookupError.set('');
    this.recipientPreview.set(null);
    this.recipientLookupVerified.set(false);

    this.authService.lookupUserByEmail(email).subscribe({
      next: (recipient) => {
        this.recipientLookupLoading.set(false);
        this.recipientPreview.set(recipient);

        if (!recipient.isActive) {
          this.recipientLookupVerified.set(false);
          this.recipientLookupError.set('Recipient account is suspended and cannot receive transfers.');
          return;
        }

        this.recipientLookupVerified.set(true);
        this.userService.getIdentityByAuthUserId(recipient.id).subscribe({
          next: (identity) => {
            const currentRecipient = this.recipientPreview();
            if (!currentRecipient) {
              return;
            }

            this.recipientPreview.set({
              ...currentRecipient,
              fullName: identity.fullName
            });
          },
          error: () => {
            // Profile lookup is optional; keep the verified recipient preview if it fails.
          }
        });
      },
      error: (err) => {
        this.recipientLookupLoading.set(false);
        this.recipientLookupVerified.set(false);
        this.recipientPreview.set(null);
        this.recipientLookupError.set(this.extractErrorMessage(err, 'Recipient not found.'));
      }
    });
  }

  resetRecipientPreview(): void {
    this.recipientPreview.set(null);
    this.recipientLookupVerified.set(false);
    this.recipientLookupError.set('');
  }

  displayRecipientName(recipient: RecipientLookup | null): string {
    if (!recipient) {
      return 'Recipient';
    }

    const source = (recipient.fullName || recipient.email.split('@')[0] || '').trim();
    if (!source) {
      return 'Recipient';
    }

    return source
      .split(/[._-]+|\s+/)
      .filter(Boolean)
      .map(part => part.charAt(0).toUpperCase() + part.slice(1).toLowerCase())
      .join(' ');
  }

  getRecipientVerificationLabel(recipient: RecipientLookup | null): string {
    if (!recipient) {
      return '';
    }

    return recipient.fullName ? 'Profile verified' : 'Account verified';
  }

  submitTopUp(): void {
    if (this.topUpForm.invalid) { this.topUpForm.markAllAsTouched(); return; }
    this.topUpLoading.set(true);
    
    // Using payment method as the description for the ledger
    const payload = {
      amount: parseFloat(this.topUpForm.value.amount),
      description: `Top-Up via ${this.topUpForm.value.paymentMethod}`
    };

    this.walletService.topUp(payload).subscribe({
      next: () => {
        this.topUpLoading.set(false);
        this.topUpOpen.set(false);
        this.topUpForm.reset({ paymentMethod: 'UPI' });
        this.toastService.success('Top-Up Successful!', `Your wallet has been credited.`);
        this.loadWallet(); // Refresh balance
      },
      error: (err) => {
        this.topUpLoading.set(false);
        this.toastService.error('Top-Up Failed', this.extractErrorMessage(err, 'An unexpected error occurred.'));
      }
    });
  }

  getTxIcon(type: string): 'credit' | 'debit' {
    return (type === 'credit' || type === 'CREDIT' || type === 'Credit') ? 'credit' : 'debit';
  }

  get canShowWalletSetup(): boolean {
    return this.hasProfile() && this.hasKyc() && this.activeTab() === 'overview';
  }

  get kycStatusLabel(): string {
    return this.kyc()?.status || 'Not Submitted';
  }

  get pinCodeControl(): AbstractControl | null {
    return this.profileForm.get('pinCode');
  }

  enforcePinCode(event: Event): void {
    const input = event.target as HTMLInputElement;
    const digits = input.value.replace(/\D/g, '').slice(0, 6);
    this.profileForm.patchValue({ pinCode: digits }, { emitEvent: false });
    input.value = digits;
  }

  formatCurrency(amount: number | null | undefined): string {
    return new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency: 'INR',
      maximumFractionDigits: 2
    }).format(amount ?? 0);
  }

  formatDate(value: string | Date | null | undefined): string {
    if (!value) {
      return 'Not available';
    }

    const date = this.parseBackendDate(value);
    if (!date) {
      return 'Not available';
    }

    return new Intl.DateTimeFormat('en-IN', {
      dateStyle: 'medium',
      timeZone: 'Asia/Kolkata'
    }).format(date);
  }

  formatDateTimeIst(value: string | Date | null | undefined): string {
    if (!value) {
      return 'Not available';
    }

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

  goToPhoneUpdate(): void {
    this.router.navigate(['/auth/update-phone']);
  }

  enforceOtpInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const digits = input.value.replace(/\D/g, '').slice(0, 6);
    this.resetPasswordForm.patchValue({ otp: digits }, { emitEvent: false });
    input.value = digits;
  }

  requestPasswordReset(): void {
    const email = this.currentUser?.email;
    if (!email) {
      this.toastService.error('Reset Unavailable', 'We could not find your account email for password reset.');
      return;
    }

    this.passwordResetSending.set(true);
    this.authService.forgotPassword({ email }).subscribe({
      next: () => {
        this.passwordResetSending.set(false);
        this.resetPasswordOpen.set(true);
        this.toastService.success(
          'Reset Code Sent',
          'A password reset OTP was sent to your registered email. For local Docker, you can view it in Mailpit.'
        );
      },
      error: (err) => {
        this.passwordResetSending.set(false);
        this.toastService.error('Reset Failed', this.extractErrorMessage(err, 'Could not start the password reset flow.'));
      }
    });
  }

  submitResetPassword(): void {
    if (this.resetPasswordForm.invalid || !this.currentUser?.email) {
      this.resetPasswordForm.markAllAsTouched();
      return;
    }

    this.resetPasswordSubmitting.set(true);
    this.authService.resetPassword({
      email: this.currentUser.email,
      otp: this.resetPasswordForm.value.otp,
      newPassword: this.resetPasswordForm.value.newPassword
    }).subscribe({
      next: () => {
        this.resetPasswordSubmitting.set(false);
        this.resetPasswordOpen.set(false);
        this.resetPasswordForm.reset();
        this.toastService.success('Password Updated', 'Your password has been changed successfully. Please use the new password next time you sign in.');
      },
      error: (err) => {
        this.resetPasswordSubmitting.set(false);
        this.toastService.error('Reset Failed', this.extractErrorMessage(err, 'Could not reset password with the provided OTP.'));
      }
    });
  }

  logout(): void {
    this.authService.logout();
    this.toastService.info('Signed Out', 'You have been logged out.');
    this.router.navigate(['/login']);
  }

  get userInitials(): string {
    const source = this.currentUser?.email?.split('@')[0] || 'User';
    return source.slice(0, 2).toUpperCase();
  }

  private extractErrorMessage(error: any, fallback: string): string {
    const payload = error?.error;

    if (typeof payload === 'string' && payload.trim()) {
      return payload;
    }

    if (payload?.message) return payload.message;
    if (payload?.Message) return payload.Message;

    if (payload?.errors) {
      // ProblemDetails format (dictionary of arrays)
      if (typeof payload.errors === 'object' && !Array.isArray(payload.errors)) {
         const firstKey = Object.keys(payload.errors)[0];
         if (firstKey && Array.isArray(payload.errors[firstKey]) && payload.errors[firstKey].length > 0) {
            return payload.errors[firstKey][0];
         }
      }
      if (Array.isArray(payload.errors) && payload.errors.length > 0) {
        return payload.errors[0];
      }
    }

    if (error?.message) {
      if (!error.message.startsWith('Http failure response')) {
         return error.message;
      }
    }

    return fallback;
  }

  private toDateInputValue(value: string): string {
    const date = this.parseBackendDate(value);
    if (!date) {
      return '';
    }

    return new Intl.DateTimeFormat('en-CA', {
      timeZone: 'Asia/Kolkata'
    }).format(date);
  }

  private noFutureDateValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null;
      }

      const selected = control.value as string;
      const today = this.getCurrentDateIst();

      return selected > today ? { futureDate: true } : null;
    };
  }

  private normalizeProfileInputs(): void {
    this.profileForm.patchValue({
      fullName: (this.profileForm.value.fullName || '').trim(),
      address: (this.profileForm.value.address || '').trim(),
      city: (this.profileForm.value.city || '').trim(),
      state: (this.profileForm.value.state || '').trim(),
      pinCode: (this.profileForm.value.pinCode || '').replace(/\D/g, '').slice(0, 6)
    });
  }

  private passwordsMatchValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const newPassword = control.get('newPassword')?.value;
      const confirmPassword = control.get('confirmPassword')?.value;

      if (!newPassword || !confirmPassword) {
        return null;
      }

      return newPassword === confirmPassword ? null : { passwordMismatch: true };
    };
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

  private getCurrentDateIst(): string {
    return new Intl.DateTimeFormat('en-CA', {
      timeZone: 'Asia/Kolkata'
    }).format(new Date());
  }
}

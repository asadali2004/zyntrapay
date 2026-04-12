import { Component, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../data/auth.service';
import { ToastService } from '../../../../shared/ui/toast/toast.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {
  registerForm: FormGroup;
  otpForm: FormGroup;
  
  step = signal<'details' | 'otp'>('details');
  loading = signal(false);
  showPassword = signal(false);
  showConfirm = signal(false);

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private toastService: ToastService,
    private router: Router
  ) {
    this.registerForm = this.fb.group({
      fullName: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', [Validators.required, Validators.pattern('^\\d{10}$')]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]],
      acceptTerms: [false, [Validators.requiredTrue]]
    }, { validators: this.passwordMatchValidator });

    this.otpForm = this.fb.group({
      otp: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  passwordMatchValidator(control: AbstractControl): { [key: string]: boolean } | null {
    const password = control.get('password');
    const confirmPassword = control.get('confirmPassword');
    if (!password || !confirmPassword) return null;
    return password.value === confirmPassword.value ? null : { passwordMismatch: true };
  }

  get passwordStrength(): number {
    const pw = this.registerForm.get('password')?.value || '';
    if (!pw) return 0;
    let score = 0;
    if (pw.length >= 8) score++;
    if (/[A-Z]/.test(pw)) score++;
    if (/[0-9]/.test(pw)) score++;
    if (/[^A-Za-z0-9]/.test(pw)) score++;
    return score;
  }

  get passwordStrengthLabel(): string {
    const s = this.passwordStrength;
    if (s === 0) return '';
    if (s === 1) return 'Weak';
    if (s === 2) return 'Fair';
    if (s === 3) return 'Good';
    return 'Strong';
  }

  get passwordStrengthClass(): string {
    const s = this.passwordStrength;
    if (s <= 1) return 'weak';
    if (s === 2) return 'fair';
    if (s === 3) return 'good';
    return 'strong';
  }

  submitDetails(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    const normalizedPhone = this.normalizePhoneNumber(this.registerForm.get('phoneNumber')?.value);
    this.registerForm.patchValue({ phoneNumber: normalizedPhone });

    this.loading.set(true);

    const email = this.registerForm.get('email')?.value;
    this.authService.requestRegistrationOtp({ email }).subscribe({
      next: () => {
        this.loading.set(false);
        this.toastService.info('OTP Sent', `Please check ${email} for your OTP.`);
        this.step.set('otp');
      },
      error: (error) => {
        this.loading.set(false);
        const msg = error.error?.message || 'Failed to send OTP.';
        this.toastService.error('Request Failed', msg);
      }
    });
  }

  submitOtp(): void {
    if (this.otpForm.invalid) {
      this.otpForm.markAllAsTouched();
      return;
    }
    
    this.loading.set(true);
    const email = this.registerForm.get('email')?.value;
    const otp = this.otpForm.get('otp')?.value;

    this.authService.verifyRegistrationOtp({ email, otp }).subscribe({
      next: () => {
        this.completeRegistration();
      },
      error: (error) => {
        this.loading.set(false);
        const msg = error.error?.message || 'Invalid OTP.';
        this.toastService.error('Verification Failed', msg);
      }
    });
  }

  private completeRegistration(): void {
    const { confirmPassword, acceptTerms, ...registerData } = this.registerForm.value;

    this.authService.register(registerData).subscribe({
      next: () => {
        this.loading.set(false);
        this.toastService.success('Account Created!', 'Redirecting to sign in...');
        setTimeout(() => this.router.navigate(['/login']), 1500);
      },
      error: (error) => {
        this.loading.set(false);
        const msg = error.error?.message || 'Registration failed.';
        this.toastService.error('Registration Failed', msg);
      }
    });
  }

  backToDetails(): void {
    this.step.set('details');
  }

  signupWithGoogle(): void {
    this.toastService.info('Coming Soon', 'Google sign-up will be available shortly.');
  }

  private normalizePhoneNumber(value: string | null | undefined): string {
    return (value ?? '').replace(/\D/g, '');
  }
}

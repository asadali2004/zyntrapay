import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../data/auth.service';
import { ToastService } from '../../../../shared/ui/toast/toast.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  loginForm: FormGroup;
  loading = signal(false);
  showPassword = signal(false);
  errorMessage = signal('');

  // Forgot password flow
  forgotMode = signal(false);
  forgotStep = signal<'email' | 'otp' | 'reset'>('email');
  forgotForm: FormGroup;
  resetForm: FormGroup;
  forgotLoading = signal(false);
  forgotEmail = signal('');

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private toastService: ToastService,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });

    this.forgotForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });

    this.resetForm = this.fb.group({
      otp: ['', [Validators.required, Validators.minLength(4)]],
      newPassword: ['', [Validators.required, Validators.minLength(6)]]
    });
  }


  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }
    this.loading.set(true);
    this.errorMessage.set('');

    this.authService.login(this.loginForm.value).subscribe({
      next: (response) => {
        this.loading.set(false);
        this.toastService.success('Welcome back!', `Signed in as ${response.email}`);
        if (response.phoneUpdateRequired) {
          this.router.navigate(['/auth/update-phone']);
        } else if (response.role === 'Admin') {
          this.router.navigate(['/admin/dashboard']);
        } else {
          this.router.navigate(['/dashboard']);
        }
      },
      error: (error) => {
        this.loading.set(false);
        const msg = error.error?.message || 'Invalid credentials. Please try again.';
        this.errorMessage.set(msg);
        this.toastService.error('Login Failed', msg);
      }
    });
  }

  openForgot(): void {
    this.forgotMode.set(true);
    this.forgotStep.set('email');
    this.forgotForm.reset();
    this.resetForm.reset();
  }

  closeForgot(): void {
    this.forgotMode.set(false);
  }

  sendForgotOtp(): void {
    if (this.forgotForm.invalid) { this.forgotForm.markAllAsTouched(); return; }
    this.forgotLoading.set(true);
    const email = this.forgotForm.value.email;
    this.forgotEmail.set(email);

    this.authService.forgotPassword({ email }).subscribe({
      next: () => {
        this.forgotLoading.set(false);
        this.forgotStep.set('otp');
        this.toastService.info('OTP Sent', `Check your email at ${email}`);
      },
      error: (err) => {
        this.forgotLoading.set(false);
        this.toastService.error('Failed', err.error?.message || 'Could not send OTP.');
      }
    });
  }

  submitReset(): void {
    if (this.resetForm.invalid) { this.resetForm.markAllAsTouched(); return; }
    this.forgotLoading.set(true);
    const { otp, newPassword } = this.resetForm.value;

    this.authService.resetPassword({ email: this.forgotEmail(), otp, newPassword }).subscribe({
      next: () => {
        this.forgotLoading.set(false);
        this.toastService.success('Password Reset!', 'You can now sign in with your new password.');
        this.closeForgot();
      },
      error: (err) => {
        this.forgotLoading.set(false);
        this.toastService.error('Failed', err.error?.message || 'Invalid OTP or expired.');
      }
    });
  }
}

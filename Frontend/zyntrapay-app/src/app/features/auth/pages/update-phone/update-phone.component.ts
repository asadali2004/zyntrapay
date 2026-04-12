import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../data/auth.service';
import { ToastService } from '../../../../shared/ui/toast/toast.service';
import { LoginResponse } from '../../models/auth.models';

@Component({
  selector: 'app-update-phone',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './update-phone.component.html',
  styleUrl: './update-phone.component.scss'
})
export class UpdatePhoneComponent {
  protected readonly loading = signal(false);
  protected readonly form;
  protected readonly currentUser: LoginResponse | null;

  constructor(
    private readonly fb: FormBuilder,
    private readonly authService: AuthService,
    private readonly toastService: ToastService,
    private readonly router: Router
  ) {
    this.currentUser = this.authService.getCurrentUser();
    this.form = this.fb.group({
      phoneNumber: ['', [Validators.required, Validators.pattern('^[0-9]{10}$')]]
    });
  }

  protected enforcePhoneInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const digits = input.value.replace(/\D/g, '').slice(0, 10);
    this.form.patchValue({ phoneNumber: digits }, { emitEvent: false });
    input.value = digits;
  }

  protected backToDashboard(): void {
    this.router.navigate(['/dashboard']);
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);

    this.authService.updatePhone({
      phoneNumber: (this.form.getRawValue().phoneNumber ?? '').trim()
    }).subscribe({
      next: () => {
        this.loading.set(false);
        this.toastService.success('Phone updated', 'Your account details were updated successfully.');
        this.router.navigate(['/dashboard']);
      },
      error: (error) => {
        this.loading.set(false);
        this.toastService.error('Update failed', error.error?.message || 'Could not update phone number.');
      }
    });
  }
}

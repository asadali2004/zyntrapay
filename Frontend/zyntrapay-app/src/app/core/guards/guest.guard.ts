import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../../features/auth/data/auth.service';

export const guestGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    return true;
  }

  return authService.isAdmin()
    ? router.createUrlTree(['/admin/dashboard'])
    : router.createUrlTree(['/dashboard']);
};

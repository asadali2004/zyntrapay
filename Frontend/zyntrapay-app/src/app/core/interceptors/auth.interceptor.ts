import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../../features/auth/data/auth.service';
import { catchError, switchMap, throwError } from 'rxjs';
import { Router } from '@angular/router';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const token = authService.getToken();
  const isRefreshRequest = req.url.includes('/auth/refresh-token');
  const isAuthPageRequest =
    req.url.includes('/auth/login') ||
    req.url.includes('/auth/register') ||
    req.url.includes('/auth/google-login') ||
    req.url.includes('/auth/forgot-password') ||
    req.url.includes('/auth/reset-password');

  if (token) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(req).pipe(
    catchError(error => {
      if (error.status === 401 && !isRefreshRequest && !isAuthPageRequest) {
        const refreshToken = authService.getRefreshToken();
        if (refreshToken) {
          return authService.refreshToken({ refreshToken }).pipe(
            switchMap(() => {
              const newToken = authService.getToken();
              req = req.clone({
                setHeaders: {
                  Authorization: `Bearer ${newToken}`
                }
              });
              return next(req);
            }),
            catchError(refreshError => {
              authService.logout();
              router.navigate(['/login']);
              return throwError(() => refreshError);
            })
          );
        } else {
          authService.logout();
          router.navigate(['/login']);
        }
      }
      return throwError(() => error);
    })
  );
};

import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

import { ToastService } from '../services/toast.service';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const toast = inject(ToastService);
  const router = inject(Router);

  const token = auth.getToken();
  const authedReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authedReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 0) {
        toast.error('Cannot reach the server. Check your connection.');
      } else if (error.status === 401) {
        auth.logout();
        router.navigate(['/login']);
      } else if (error.status === 403) {
        toast.error('Access denied. You do not have permission for this action.');
      } else if (error.status >= 500) {
        toast.error('Something went wrong on the server. Please try again.');
      }
      return throwError(() => error);
    }),
  );
};

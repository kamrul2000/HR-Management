import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  heroEye,
  heroEyeSlash,
  heroArrowRight,
  heroBuildingOffice2,
  heroLockClosed,
  heroChartBar,
  heroUsers,
} from '@ng-icons/heroicons/outline';

import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'hrm-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, NgIcon],
  providers: [
    provideIcons({
      heroEye,
      heroEyeSlash,
      heroArrowRight,
      heroBuildingOffice2,
      heroLockClosed,
      heroChartBar,
      heroUsers,
    }),
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    rememberMe: [true],
  });

  readonly showPassword = signal(false);
  readonly loading = signal(false);
  readonly inlineError = signal<string | null>(null);
  readonly year = new Date().getFullYear();

  readonly canSubmit = computed(() => !this.loading());

  togglePassword(): void {
    this.showPassword.update((v) => !v);
  }

  clearError(): void {
    if (this.inlineError()) this.inlineError.set(null);
  }

  submit(): void {
    if (this.form.invalid || this.loading()) {
      this.form.markAllAsTouched();
      return;
    }

    const { email, password } = this.form.getRawValue();

    this.loading.set(true);
    this.inlineError.set(null);

    this.auth.login({ email, password }).subscribe({
      next: (res) => {
        if (res.success) {
          const redirectTo =
            this.route.snapshot.queryParamMap.get('redirectTo') ?? '/dashboard';
          this.router.navigateByUrl(redirectTo);
        } else {
          this.inlineError.set(res.message || 'Login failed.');
          this.loading.set(false);
        }
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        if (err.status === 401) {
          this.inlineError.set(err.error?.message || 'Invalid email or password.');
        } else if (err.status === 400) {
          this.inlineError.set(err.error?.message || 'Please check your credentials.');
        } else if (err.status !== 0 && err.status < 500) {
          this.inlineError.set(err.error?.message || 'Could not sign in.');
        }
        // status 0 and 5xx are surfaced by the interceptor toast.
      },
    });
  }

  hasError(field: 'email' | 'password', error: string): boolean {
    const ctrl = this.form.controls[field];
    return ctrl.touched && ctrl.hasError(error);
  }
}

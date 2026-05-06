import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { AuthUser, JwtPayload, LoginRequest, LoginResponse } from './auth.models';

const TOKEN_KEY = `${environment.storageKeyPrefix}token`;
const USER_KEY = `${environment.storageKeyPrefix}user`;

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/auth`;

  private readonly _user = signal<AuthUser | null>(this.restoreUser());
  readonly user = this._user.asReadonly();
  readonly isAuthenticated = computed(() => !!this._user() && !this.isTokenExpired());

  login(request: LoginRequest): Observable<ApiResponse<LoginResponse>> {
    return this.http
      .post<ApiResponse<LoginResponse>>(`${this.baseUrl}/login`, request)
      .pipe(
        tap((res) => {
          if (res.success && res.data) {
            this.persistSession(res.data);
          }
        }),
      );
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this._user.set(null);
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  hasRole(role: string): boolean {
    const user = this._user();
    return user?.role?.toLowerCase() === role.toLowerCase();
  }

  private persistSession(payload: LoginResponse): void {
    localStorage.setItem(TOKEN_KEY, payload.token);

    const fromPayload: AuthUser = payload.user ?? this.userFromToken(payload.token);
    localStorage.setItem(USER_KEY, JSON.stringify(fromPayload));
    this._user.set(fromPayload);
  }

  private restoreUser(): AuthUser | null {
    const raw = localStorage.getItem(USER_KEY);
    if (!raw) return null;
    try {
      const user = JSON.parse(raw) as AuthUser;
      if (this.isTokenExpired()) {
        this.logout();
        return null;
      }
      return user;
    } catch {
      return null;
    }
  }

  private userFromToken(token: string): AuthUser {
    const claims = decodeJwt(token);
    return {
      id: Number(claims.userId ?? 0),
      email: String(claims.email ?? ''),
      name: String((claims['name'] as string | undefined) ?? claims.email ?? ''),
      role: claims.role,
      subscriptionId: Number(claims.subscriptionId ?? 0),
    };
  }

  isTokenExpired(): boolean {
    const token = this.getToken();
    if (!token) return true;
    const claims = decodeJwt(token);
    if (!claims.exp) return false;
    return claims.exp * 1000 < Date.now();
  }
}

export function decodeJwt(token: string): JwtPayload {
  try {
    const parts = token.split('.');
    if (parts.length !== 3) return {};
    const payload = parts[1].replace(/-/g, '+').replace(/_/g, '/');
    const padded = payload + '='.repeat((4 - (payload.length % 4)) % 4);
    const decoded = atob(padded);
    return JSON.parse(decoded) as JwtPayload;
  } catch {
    return {};
  }
}

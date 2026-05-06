import { Injectable, signal } from '@angular/core';
import { Observable, Subject } from 'rxjs';

export interface ConfirmOptions {
  title: string;
  message: string;
  confirmLabel?: string;
  cancelLabel?: string;
  danger?: boolean;
}

export interface ConfirmRequest extends Required<ConfirmOptions> {
  id: number;
  result$: Subject<boolean>;
}

@Injectable({ providedIn: 'root' })
export class ConfirmService {
  private readonly _request = signal<ConfirmRequest | null>(null);
  readonly request = this._request.asReadonly();

  confirm(options: ConfirmOptions): Observable<boolean> {
    const result$ = new Subject<boolean>();
    this._request.set({
      id: Date.now(),
      title: options.title,
      message: options.message,
      confirmLabel: options.confirmLabel ?? 'Confirm',
      cancelLabel: options.cancelLabel ?? 'Cancel',
      danger: options.danger ?? false,
      result$,
    });
    return result$.asObservable();
  }

  resolve(value: boolean): void {
    const current = this._request();
    if (!current) return;
    current.result$.next(value);
    current.result$.complete();
    this._request.set(null);
  }
}

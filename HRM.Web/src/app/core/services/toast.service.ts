import { Injectable, signal } from '@angular/core';

export type ToastType = 'success' | 'error' | 'warning' | 'info';

export interface Toast {
  id: number;
  type: ToastType;
  message: string;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly _toasts = signal<Toast[]>([]);
  readonly toasts = this._toasts.asReadonly();

  success(message: string, duration = 4000): void {
    this.add('success', message, duration);
  }

  error(message: string, duration = 6000): void {
    this.add('error', message, duration);
  }

  warning(message: string, duration = 5000): void {
    this.add('warning', message, duration);
  }

  info(message: string, duration = 4000): void {
    this.add('info', message, duration);
  }

  remove(id: number): void {
    this._toasts.update((items) => items.filter((t) => t.id !== id));
  }

  private add(type: ToastType, message: string, duration: number): void {
    const id = Date.now() + Math.floor(Math.random() * 1000);
    this._toasts.update((items) => [...items, { id, type, message }]);
    if (duration > 0) {
      setTimeout(() => this.remove(id), duration);
    }
  }
}

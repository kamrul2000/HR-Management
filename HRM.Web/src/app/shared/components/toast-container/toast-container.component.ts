import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  heroCheckCircle,
  heroExclamationCircle,
  heroInformationCircle,
  heroExclamationTriangle,
  heroXMark,
} from '@ng-icons/heroicons/outline';

import { ToastService, ToastType } from '../../../core/services/toast.service';

@Component({
  selector: 'hrm-toast-container',
  standalone: true,
  imports: [CommonModule, NgIcon],
  providers: [
    provideIcons({
      heroCheckCircle,
      heroExclamationCircle,
      heroInformationCircle,
      heroExclamationTriangle,
      heroXMark,
    }),
  ],
  templateUrl: './toast-container.component.html',
  styleUrl: './toast-container.component.scss',
})
export class ToastContainerComponent {
  private readonly toastService = inject(ToastService);
  readonly toasts = this.toastService.toasts;

  iconFor(type: ToastType): string {
    switch (type) {
      case 'success': return 'heroCheckCircle';
      case 'error':   return 'heroExclamationCircle';
      case 'warning': return 'heroExclamationTriangle';
      default:        return 'heroInformationCircle';
    }
  }

  dismiss(id: number): void {
    this.toastService.remove(id);
  }
}

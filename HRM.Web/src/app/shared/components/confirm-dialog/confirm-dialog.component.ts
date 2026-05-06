import { CommonModule } from '@angular/common';
import { Component, HostListener, inject } from '@angular/core';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  heroExclamationTriangle,
  heroQuestionMarkCircle,
} from '@ng-icons/heroicons/outline';

import { ConfirmService } from '../../../core/services/confirm.service';

@Component({
  selector: 'hrm-confirm-dialog',
  standalone: true,
  imports: [CommonModule, NgIcon],
  providers: [provideIcons({ heroExclamationTriangle, heroQuestionMarkCircle })],
  templateUrl: './confirm-dialog.component.html',
  styleUrl: './confirm-dialog.component.scss',
})
export class ConfirmDialogComponent {
  protected readonly confirm = inject(ConfirmService);

  cancel(): void {
    this.confirm.resolve(false);
  }

  accept(): void {
    this.confirm.resolve(true);
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.confirm.request()) {
      this.cancel();
    }
  }
}

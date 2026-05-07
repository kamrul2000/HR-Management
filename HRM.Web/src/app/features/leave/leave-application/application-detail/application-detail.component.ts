import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  EventEmitter,
  Input,
  Output,
  inject,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  heroCheck,
  heroXMark,
  heroPaperClip,
  heroDocumentArrowDown,
} from '@ng-icons/heroicons/outline';

import { ConfirmService } from '../../../../core/services/confirm.service';
import { ToastService } from '../../../../core/services/toast.service';
import { DrawerComponent } from '../../../../shared/components/drawer/drawer.component';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { LeaveApplicationResponse } from '../../models/leave-application.model';
import { LeaveApplicationService } from '../../services/leave-application.service';

@Component({
  selector: 'hrm-application-detail',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NgIcon,
    DrawerComponent,
    StatusBadgeComponent,
  ],
  providers: [provideIcons({ heroCheck, heroXMark, heroPaperClip, heroDocumentArrowDown })],
  templateUrl: './application-detail.component.html',
  styleUrl: './application-detail.component.scss',
})
export class ApplicationDetailComponent {
  private readonly service = inject(LeaveApplicationService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmService);

  @Input({ required: true }) application!: LeaveApplicationResponse;

  @Output() updated = new EventEmitter<LeaveApplicationResponse>();
  @Output() dismiss = new EventEmitter<void>();

  readonly busy = signal(false);
  readonly approvalRemarks = signal('');
  readonly cancellationReason = signal('');

  get isPending(): boolean { return this.application.status === 'Pending'; }

  approve(): void {
    if (this.busy()) return;
    this.busy.set(true);
    this.service.approve(this.application.id, { approvalRemarks: this.approvalRemarks().trim() || null }).subscribe({
      next: (res) => {
        this.busy.set(false);
        if (res.success && res.data) {
          this.toast.success('Application approved.');
          this.updated.emit(res.data);
        } else {
          this.toast.error(res.message || 'Failed to approve.');
        }
      },
      error: (err: HttpErrorResponse) => {
        this.busy.set(false);
        this.toast.error(err.error?.message || 'Failed to approve.');
      },
    });
  }

  reject(): void {
    if (this.busy()) return;
    const remarks = this.approvalRemarks().trim();
    if (!remarks) {
      this.toast.error('Provide a reason before rejecting.');
      return;
    }
    this.busy.set(true);
    this.service.reject(this.application.id, { approvalRemarks: remarks }).subscribe({
      next: (res) => {
        this.busy.set(false);
        if (res.success && res.data) {
          this.toast.success('Application rejected.');
          this.updated.emit(res.data);
        } else {
          this.toast.error(res.message || 'Failed to reject.');
        }
      },
      error: (err: HttpErrorResponse) => {
        this.busy.set(false);
        this.toast.error(err.error?.message || 'Failed to reject.');
      },
    });
  }

  cancelApplication(): void {
    if (this.busy()) return;
    const reason = this.cancellationReason().trim();
    if (!reason) {
      this.toast.error('Provide a cancellation reason.');
      return;
    }
    this.confirm
      .confirm({
        title: 'Cancel application',
        message: 'This action cannot be undone. Cancel this leave application?',
        confirmLabel: 'Cancel application',
        danger: true,
      })
      .subscribe((ok) => {
        if (!ok) return;
        this.busy.set(true);
        this.service.cancel(this.application.id, { cancellationReason: reason }).subscribe({
          next: (res) => {
            this.busy.set(false);
            if (res.success && res.data) {
              this.toast.success('Application cancelled.');
              this.updated.emit(res.data);
            } else {
              this.toast.error(res.message || 'Failed to cancel.');
            }
          },
          error: (err: HttpErrorResponse) => {
            this.busy.set(false);
            this.toast.error(err.error?.message || 'Failed to cancel.');
          },
        });
      });
  }

  onAttachmentChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) return;
    this.busy.set(true);
    this.service.uploadAttachment(this.application.id, file).subscribe({
      next: (res) => {
        this.busy.set(false);
        if (res.success && res.data) {
          this.toast.success('Attachment uploaded.');
          this.updated.emit(res.data);
        } else {
          this.toast.error(res.message || 'Failed to upload.');
        }
      },
      error: (err: HttpErrorResponse) => {
        this.busy.set(false);
        this.toast.error(err.error?.message || 'Failed to upload.');
      },
    });
  }
}

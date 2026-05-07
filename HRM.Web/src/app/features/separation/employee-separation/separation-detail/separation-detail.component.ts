import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  heroArrowLeft,
  heroCheck,
  heroXMark,
  heroPaperAirplane,
  heroDocumentText,
} from '@ng-icons/heroicons/outline';

import { ConfirmService } from '../../../../core/services/confirm.service';
import { ToastService } from '../../../../core/services/toast.service';
import { AvatarComponent } from '../../../../shared/components/avatar/avatar.component';
import { DrawerComponent } from '../../../../shared/components/drawer/drawer.component';
import { LoadingSkeletonComponent } from '../../../../shared/components/loading-skeleton/loading-skeleton.component';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { CurrencyBdPipe } from '../../../../shared/pipes/currency-bd.pipe';
import { SeparationResponse } from '../../models/employee-separation.model';
import { EmployeeSeparationService } from '../../services/employee-separation.service';

interface TimelineEvent {
  key: string;
  title: string;
  date?: string | null;
  description?: string;
  state: 'done' | 'current' | 'rejected' | 'pending';
}

type ActionState =
  | { kind: 'closed' }
  | { kind: 'approve' }
  | { kind: 'cancel' };

@Component({
  selector: 'hrm-separation-detail',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    NgIcon,
    PageHeaderComponent,
    LoadingSkeletonComponent,
    AvatarComponent,
    StatusBadgeComponent,
    CurrencyBdPipe,
    DrawerComponent,
  ],
  providers: [provideIcons({ heroArrowLeft, heroCheck, heroXMark, heroPaperAirplane, heroDocumentText })],
  templateUrl: './separation-detail.component.html',
  styleUrls: ['../../../loan/loan-application/application-detail/application-detail.component.scss'],
})
export class SeparationDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmService);
  private readonly service = inject(EmployeeSeparationService);

  readonly record = signal<SeparationResponse | null>(null);
  readonly loading = signal(true);
  readonly busy = signal(false);

  readonly action = signal<ActionState>({ kind: 'closed' });
  readonly remarks = signal<string>('');
  readonly cancelReason = signal<string>('');

  readonly canApprove = computed(() => this.record()?.status === 'Pending');
  readonly canProcess = computed(() => this.record()?.status === 'Approved');
  readonly canCancel = computed(() => {
    const s = this.record()?.status;
    return s === 'Pending' || s === 'Approved';
  });

  readonly timeline = computed<TimelineEvent[]>(() => {
    const r = this.record();
    if (!r) return [];
    const events: TimelineEvent[] = [];
    events.push({
      key: 'applied',
      title: 'Separation initiated',
      date: r.createdAt,
      description: `${r.separationTypeLabel || r.separationType} · ${r.separationReasonName}. Last working day ${r.lastWorkingDateFormatted || r.lastWorkingDate}.`,
      state: 'done',
    });
    if (r.approvalDate) {
      events.push({
        key: 'approved',
        title: 'Approved',
        date: r.approvalDate,
        description: r.approvalRemarks || undefined,
        state: 'done',
      });
    } else if (r.status === 'Pending') {
      events.push({ key: 'awaiting-approval', title: 'Awaiting approval', state: 'current' });
    }
    if (r.processedDate) {
      events.push({
        key: 'processed',
        title: 'Settlement processed',
        date: r.processedDate,
        description: 'Final settlement disbursed and employee status updated.',
        state: 'done',
      });
    } else if (r.status === 'Approved') {
      events.push({ key: 'awaiting-process', title: 'Awaiting settlement processing', state: 'current' });
    }
    if (r.status === 'Cancelled') {
      events.push({
        key: 'cancelled',
        title: 'Separation cancelled',
        date: r.updatedAt,
        state: 'rejected',
      });
    }
    return events;
  });

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) {
      this.router.navigate(['/separation']);
      return;
    }
    this.load(id);
  }

  private load(id: number): void {
    this.loading.set(true);
    this.service.getById(id).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) this.record.set(res.data);
        else this.router.navigate(['/separation']);
      },
      error: () => {
        this.loading.set(false);
        this.router.navigate(['/separation']);
      },
    });
  }

  openApprove(): void {
    this.remarks.set('');
    this.action.set({ kind: 'approve' });
  }

  openCancel(): void {
    this.cancelReason.set('');
    this.action.set({ kind: 'cancel' });
  }

  closeAction(): void { this.action.set({ kind: 'closed' }); }

  confirmApprove(): void {
    const id = this.record()?.id;
    if (!id || this.busy()) return;
    this.busy.set(true);
    this.service.approve(id, { approvalRemarks: this.remarks().trim() || null }).subscribe({
      next: (res) => this.afterAction(res, 'Separation approved.'),
      error: (err: HttpErrorResponse) => this.afterError(err),
    });
  }

  confirmCancel(): void {
    const id = this.record()?.id;
    if (!id || this.busy()) return;
    const reason = this.cancelReason().trim();
    if (!reason) {
      this.toast.error('Provide a reason.');
      return;
    }
    this.busy.set(true);
    this.service.cancel(id, { cancellationReason: reason }).subscribe({
      next: (res) => this.afterAction(res, 'Separation cancelled.'),
      error: (err: HttpErrorResponse) => this.afterError(err),
    });
  }

  process(): void {
    const id = this.record()?.id;
    if (!id) return;
    this.confirm.confirm({
      title: 'Process settlement',
      message: 'Disburse the final settlement and mark the employee as separated?',
      confirmLabel: 'Process Settlement',
    }).subscribe((ok) => {
      if (!ok) return;
      this.busy.set(true);
      this.service.process(id).subscribe({
        next: (res) => this.afterAction(res, 'Settlement processed.'),
        error: (err: HttpErrorResponse) => this.afterError(err),
      });
    });
  }

  private afterAction(
    res: { success: boolean; message: string; data: SeparationResponse | null },
    msg: string,
  ): void {
    this.busy.set(false);
    if (res.success && res.data) {
      this.toast.success(msg);
      this.record.set(res.data);
      this.closeAction();
    } else {
      this.toast.error(res.message || 'Action failed.');
    }
  }

  private afterError(err: HttpErrorResponse): void {
    this.busy.set(false);
    this.toast.error(err.error?.message || 'Action failed.');
  }
}

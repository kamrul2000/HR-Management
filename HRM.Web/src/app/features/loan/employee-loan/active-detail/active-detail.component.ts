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
  heroArrowUturnLeft,
  heroBanknotes,
} from '@ng-icons/heroicons/outline';

import { ConfirmService } from '../../../../core/services/confirm.service';
import { ToastService } from '../../../../core/services/toast.service';
import { AvatarComponent } from '../../../../shared/components/avatar/avatar.component';
import { DrawerComponent } from '../../../../shared/components/drawer/drawer.component';
import { LoadingSkeletonComponent } from '../../../../shared/components/loading-skeleton/loading-skeleton.component';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { CurrencyBdPipe } from '../../../../shared/pipes/currency-bd.pipe';
import { EmployeeLoanResponse } from '../../models/employee-loan.model';
import { LoanInstallmentResponse } from '../../models/loan-installment.model';
import { EmployeeLoanService } from '../../services/employee-loan.service';
import { LoanInstallmentService } from '../../services/loan-installment.service';

interface ActionState {
  kind: 'closed' | 'pay' | 'skip';
  inst?: LoanInstallmentResponse;
}

@Component({
  selector: 'hrm-active-loan-detail',
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
  providers: [provideIcons({ heroArrowLeft, heroCheck, heroXMark, heroArrowUturnLeft, heroBanknotes })],
  templateUrl: './active-detail.component.html',
  styleUrl: './active-detail.component.scss',
})
export class ActiveDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmService);
  private readonly loans = inject(EmployeeLoanService);
  private readonly installments = inject(LoanInstallmentService);

  readonly loan = signal<EmployeeLoanResponse | null>(null);
  readonly schedule = signal<LoanInstallmentResponse[]>([]);
  readonly loading = signal(true);

  readonly action = signal<ActionState>({ kind: 'closed' });
  readonly payAmount = signal<number | null>(null);
  readonly remarks = signal<string>('');
  readonly busy = signal(false);

  readonly nextDue = computed<LoanInstallmentResponse | null>(() => {
    const list = this.schedule();
    return list.find((x) => x.status === 'Pending' || x.status === 'Overdue') ?? null;
  });

  readonly actionOpen = computed(() => this.action().kind !== 'closed');
  readonly actionKind = computed(() => this.action().kind);
  readonly actionInst = computed<LoanInstallmentResponse | null>(() => this.action().inst ?? null);

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) {
      this.toast.error('Invalid loan id.');
      this.router.navigate(['/loans/active']);
      return;
    }
    this.load(id);
  }

  private load(id: number): void {
    this.loading.set(true);
    this.loans.getById(id).subscribe({
      next: (res) => {
        if (!res.success || !res.data) {
          this.loading.set(false);
          this.router.navigate(['/loans/active']);
          return;
        }
        this.loan.set(res.data);
        this.installments.getByLoan(id).subscribe({
          next: (sched) => {
            this.loading.set(false);
            if (sched.success && sched.data) this.schedule.set(sched.data);
          },
          error: () => this.loading.set(false),
        });
      },
      error: () => {
        this.loading.set(false);
        this.router.navigate(['/loans/active']);
      },
    });
  }

  openPay(inst: LoanInstallmentResponse): void {
    this.action.set({ kind: 'pay', inst });
    this.payAmount.set(inst.installmentAmount);
    this.remarks.set('');
  }

  openSkip(inst: LoanInstallmentResponse): void {
    this.action.set({ kind: 'skip', inst });
    this.remarks.set('');
  }

  closeAction(): void { this.action.set({ kind: 'closed' }); }

  confirmPay(): void {
    const a = this.action();
    if (a.kind !== 'pay' || !a.inst || this.busy()) return;
    this.busy.set(true);
    this.installments.pay(a.inst.id, {
      paidAmount: this.payAmount(),
      remarks: this.remarks().trim() || null,
    }).subscribe({
      next: (res) => this.afterAction(res, 'Installment paid.'),
      error: (err: HttpErrorResponse) => this.afterError(err),
    });
  }

  confirmSkip(): void {
    const a = this.action();
    if (a.kind !== 'skip' || !a.inst || this.busy()) return;
    if (!this.remarks().trim()) {
      this.toast.error('Provide a reason for skipping.');
      return;
    }
    this.busy.set(true);
    this.installments.skip(a.inst.id, { reason: this.remarks().trim() }).subscribe({
      next: (res) => this.afterAction(res, 'Installment skipped.'),
      error: (err: HttpErrorResponse) => this.afterError(err),
    });
  }

  reinstate(inst: LoanInstallmentResponse): void {
    this.confirm.confirm({
      title: 'Reinstate installment',
      message: `Reset installment #${inst.installmentNo} (${inst.duePeriodLabel}) to Pending?`,
      confirmLabel: 'Reinstate',
    }).subscribe((ok) => {
      if (!ok) return;
      this.installments.reinstate(inst.id).subscribe({
        next: (res) => {
          if (res.success && res.data) {
            this.toast.success('Installment reinstated.');
            this.replaceInstallment(res.data);
            this.refreshLoan();
          } else {
            this.toast.error(res.message || 'Failed.');
          }
        },
        error: (err) => this.toast.error(err.error?.message || 'Failed.'),
      });
    });
  }

  completeLoan(): void {
    const id = this.loan()?.id;
    if (!id) return;
    this.confirm.confirm({
      title: 'Mark loan completed',
      message: 'Force-complete this loan even if some installments remain?',
      confirmLabel: 'Mark Completed',
    }).subscribe((ok) => {
      if (!ok) return;
      this.loans.complete(id).subscribe({
        next: (res) => {
          if (res.success && res.data) {
            this.toast.success('Loan completed.');
            this.loan.set(res.data);
          } else {
            this.toast.error(res.message || 'Failed.');
          }
        },
        error: (err) => this.toast.error(err.error?.message || 'Failed.'),
      });
    });
  }

  defaultLoan(): void {
    const id = this.loan()?.id;
    if (!id) return;
    const reason = window.prompt('Reason for marking as defaulted:');
    if (!reason || !reason.trim()) return;
    this.loans.default(id, reason.trim()).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.toast.success('Loan marked as defaulted.');
          this.loan.set(res.data);
        } else {
          this.toast.error(res.message || 'Failed.');
        }
      },
      error: (err) => this.toast.error(err.error?.message || 'Failed.'),
    });
  }

  private afterAction(
    res: { success: boolean; message: string; data: LoanInstallmentResponse | null },
    msg: string,
  ): void {
    this.busy.set(false);
    if (res.success && res.data) {
      this.toast.success(msg);
      this.replaceInstallment(res.data);
      this.refreshLoan();
      this.closeAction();
    } else {
      this.toast.error(res.message || 'Action failed.');
    }
  }

  private afterError(err: HttpErrorResponse): void {
    this.busy.set(false);
    this.toast.error(err.error?.message || 'Action failed.');
  }

  private replaceInstallment(updated: LoanInstallmentResponse): void {
    this.schedule.set(this.schedule().map((x) => (x.id === updated.id ? updated : x)));
  }

  private refreshLoan(): void {
    const id = this.loan()?.id;
    if (!id) return;
    this.loans.getById(id).subscribe({
      next: (res) => {
        if (res.success && res.data) this.loan.set(res.data);
      },
    });
  }
}

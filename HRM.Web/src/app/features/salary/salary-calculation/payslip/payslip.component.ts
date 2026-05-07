import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  heroArrowLeft,
  heroPrinter,
  heroCheck,
  heroXMark,
} from '@ng-icons/heroicons/outline';

import { ConfirmService } from '../../../../core/services/confirm.service';
import { ToastService } from '../../../../core/services/toast.service';
import { LoadingSkeletonComponent } from '../../../../shared/components/loading-skeleton/loading-skeleton.component';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { CurrencyBdPipe } from '../../../../shared/pipes/currency-bd.pipe';
import { SalaryCalculationResponse } from '../../models/salary-calculation.model';
import { SalaryCalculationService } from '../../services/salary-calculation.service';

@Component({
  selector: 'hrm-payslip',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    NgIcon,
    PageHeaderComponent,
    LoadingSkeletonComponent,
    StatusBadgeComponent,
    CurrencyBdPipe,
  ],
  providers: [provideIcons({ heroArrowLeft, heroPrinter, heroCheck, heroXMark })],
  templateUrl: './payslip.component.html',
  styleUrl: './payslip.component.scss',
})
export class PayslipComponent implements OnInit {
  private readonly service = inject(SalaryCalculationService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmService);

  readonly calc = signal<SalaryCalculationResponse | null>(null);
  readonly loading = signal(true);
  readonly busy = signal(false);

  readonly isDraft = computed(() => this.calc()?.status === 'Draft');
  readonly isFinalized = computed(() => this.calc()?.status === 'Finalized');

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) {
      this.router.navigate(['/salary/calculations']);
      return;
    }
    this.load(id);
  }

  load(id: number): void {
    this.loading.set(true);
    this.service.getById(id).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) this.calc.set(res.data);
        else this.router.navigate(['/salary/calculations']);
      },
      error: () => {
        this.loading.set(false);
        this.router.navigate(['/salary/calculations']);
      },
    });
  }

  finalize(): void {
    const c = this.calc();
    if (!c || this.busy()) return;
    this.confirm
      .confirm({
        title: 'Finalize calculation',
        message: 'Once finalized, the payslip is locked and downstream actions (loan installments, etc.) will fire.',
        confirmLabel: 'Finalize',
      })
      .subscribe((ok) => {
        if (!ok) return;
        this.busy.set(true);
        this.service.finalize(c.id).subscribe({
          next: (res) => this.handleResult(res, 'Calculation finalized.'),
          error: (err: HttpErrorResponse) => this.handleError(err),
        });
      });
  }

  cancel(): void {
    const c = this.calc();
    if (!c || this.busy()) return;
    const reason = prompt('Cancellation reason:');
    if (!reason || !reason.trim()) return;

    this.busy.set(true);
    this.service.cancel(c.id, reason.trim()).subscribe({
      next: (res) => this.handleResult(res, 'Calculation cancelled.'),
      error: (err: HttpErrorResponse) => this.handleError(err),
    });
  }

  private handleResult(
    res: { success: boolean; message: string; data: SalaryCalculationResponse | null },
    msg: string,
  ): void {
    this.busy.set(false);
    if (res.success && res.data) {
      this.toast.success(msg);
      this.calc.set(res.data);
    } else {
      this.toast.error(res.message || 'Action failed.');
    }
  }

  private handleError(err: HttpErrorResponse): void {
    this.busy.set(false);
    this.toast.error(err.error?.message || 'Action failed.');
  }

  print(): void {
    window.print();
  }
}

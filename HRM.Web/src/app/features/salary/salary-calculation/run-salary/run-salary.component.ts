import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  heroArrowLeft,
  heroPlay,
  heroCheckCircle,
} from '@ng-icons/heroicons/outline';

import { ToastService } from '../../../../core/services/toast.service';
import { BulkCreateResult } from '../../../../core/models/api-response.model';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { BranchResponse } from '../../../organization/models/branch.model';
import { BranchService } from '../../../organization/services/branch.service';
import { SalaryCalculationService } from '../../services/salary-calculation.service';

type Scope = 'all' | 'branch';

@Component({
  selector: 'hrm-run-salary',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    NgIcon,
    PageHeaderComponent,
  ],
  providers: [provideIcons({ heroArrowLeft, heroPlay, heroCheckCircle })],
  templateUrl: './run-salary.component.html',
  styleUrl: './run-salary.component.scss',
})
export class RunSalaryComponent implements OnInit {
  private readonly service = inject(SalaryCalculationService);
  private readonly branches = inject(BranchService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);

  readonly month = signal<number>(new Date().getMonth() + 1);
  readonly year = signal<number>(new Date().getFullYear());
  readonly scope = signal<Scope>('all');
  readonly branchId = signal<number | null>(null);
  readonly remarks = signal<string>('');
  readonly running = signal(false);
  readonly result = signal<BulkCreateResult | null>(null);

  readonly branchOptions = signal<BranchResponse[]>([]);
  readonly months = Array.from({ length: 12 }, (_, i) => ({
    value: i + 1,
    label: new Date(2000, i, 1).toLocaleDateString('en-GB', { month: 'long' }),
  }));

  readonly canSubmit = computed(() => {
    if (this.running()) return false;
    if (!this.month() || !this.year()) return false;
    if (this.scope() === 'branch') return !!this.branchId();
    return true;
  });

  ngOnInit(): void {
    this.branches.getAll({ pageSize: 200, isActive: true }).subscribe({
      next: (res) => {
        if (res.success && res.data) this.branchOptions.set(res.data.items);
      },
    });
  }

  setScope(s: Scope): void {
    this.scope.set(s);
    if (s === 'all') this.branchId.set(null);
  }

  run(): void {
    if (!this.canSubmit()) return;

    this.running.set(true);
    this.result.set(null);

    this.service
      .bulkCalculate({
        year: this.year(),
        month: this.month(),
        branchId: this.scope() === 'branch' ? this.branchId() ?? undefined : undefined,
        remarks: this.remarks().trim() || null,
      })
      .subscribe({
        next: (res) => {
          this.running.set(false);
          if (res.success && res.data) {
            this.result.set(res.data);
            this.toast.success('Payroll run completed.');
          } else {
            this.toast.error(res.message || 'Payroll run failed.');
          }
        },
        error: (err: HttpErrorResponse) => {
          this.running.set(false);
          this.toast.error(err.error?.message || 'Payroll run failed.');
        },
      });
  }

  startOver(): void { this.result.set(null); }

  goToList(): void {
    this.router.navigate(['/salary/calculations']);
  }
}

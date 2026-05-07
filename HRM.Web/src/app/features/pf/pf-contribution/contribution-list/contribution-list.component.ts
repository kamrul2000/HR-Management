import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  OnInit,
  TemplateRef,
  ViewChild,
  computed,
  inject,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { heroPlay, heroBookOpen, heroBriefcase } from '@ng-icons/heroicons/outline';

import { ConfirmService } from '../../../../core/services/confirm.service';
import { ToastService } from '../../../../core/services/toast.service';
import { AvatarComponent } from '../../../../shared/components/avatar/avatar.component';
import { DataTableComponent } from '../../../../shared/components/data-table/data-table.component';
import {
  PageState,
  TableColumn,
} from '../../../../shared/components/data-table/data-table.types';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { CurrencyBdPipe } from '../../../../shared/pipes/currency-bd.pipe';
import {
  EmployeePfContributionResponse,
  PfMonthlyReport,
} from '../../models/pf-contribution.model';
import { PfContributionService } from '../../services/pf-contribution.service';
import { PfRuleListComponent } from '../pf-rule-list/pf-rule-list.component';

@Component({
  selector: 'hrm-pf-contribution-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NgIcon,
    PageHeaderComponent,
    DataTableComponent,
    AvatarComponent,
    CurrencyBdPipe,
    PfRuleListComponent,
  ],
  providers: [provideIcons({ heroPlay, heroBookOpen, heroBriefcase })],
  templateUrl: './contribution-list.component.html',
  styles: [
    `
      .stat-strip {
        display: grid;
        grid-template-columns: repeat(4, 1fr);
        gap: 12px;
        margin-bottom: 16px;
      }
      .stat-card {
        background: #fff;
        border: 1px solid #E2E8F0;
        border-radius: 8px;
        padding: 14px 16px;
      }
      .stat-card__label { font-size: 12px; color: #64748B; text-transform: uppercase; letter-spacing: 0.4px; }
      .stat-card__value { font-weight: 600; color: #0F172A; font-size: 18px; margin-top: 4px; }
      .stat-card--employee .stat-card__value { color: #2563EB; }
      .stat-card--employer .stat-card__value { color: #16A34A; }
      .stat-card--total    .stat-card__value { color: #7C3AED; }
    `,
  ],
})
export class ContributionListComponent implements OnInit {
  private readonly service = inject(PfContributionService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmService);

  readonly rows = signal<EmployeePfContributionResponse[]>([]);
  readonly totalCount = signal(0);
  readonly loading = signal(true);
  readonly busy = signal(false);
  readonly report = signal<PfMonthlyReport | null>(null);
  readonly rulesOpen = signal(false);

  readonly month = signal<number>(new Date().getMonth() + 1);
  readonly year = signal<number>(new Date().getFullYear());
  readonly pageNumber = signal(1);
  readonly pageSize = signal(20);

  readonly months = Array.from({ length: 12 }, (_, i) => ({
    value: i + 1,
    label: new Date(2000, i, 1).toLocaleDateString('en-GB', { month: 'long' }),
  }));

  readonly pageState = computed<PageState>(() => ({
    pageNumber: this.pageNumber(),
    pageSize: this.pageSize(),
    totalCount: this.totalCount(),
  }));

  @ViewChild('employeeCellTpl', { static: true }) employeeCellTpl!: TemplateRef<{ $implicit: EmployeePfContributionResponse }>;
  @ViewChild('baseCellTpl',     { static: true }) baseCellTpl!:     TemplateRef<{ $implicit: EmployeePfContributionResponse }>;
  @ViewChild('empCellTpl',      { static: true }) empCellTpl!:      TemplateRef<{ $implicit: EmployeePfContributionResponse }>;
  @ViewChild('emprCellTpl',     { static: true }) emprCellTpl!:     TemplateRef<{ $implicit: EmployeePfContributionResponse }>;
  @ViewChild('totalCellTpl',    { static: true }) totalCellTpl!:    TemplateRef<{ $implicit: EmployeePfContributionResponse }>;

  columns: TableColumn<EmployeePfContributionResponse>[] = [];

  ngOnInit(): void {
    this.columns = [
      { key: 'employee', label: 'Employee', template: this.employeeCellTpl },
      { key: 'rule',     label: 'PF Rule', width: '180px' },
      { key: 'base',     label: 'PF Base', template: this.baseCellTpl, align: 'right', width: '130px' },
      { key: 'emp',      label: 'Employee', template: this.empCellTpl, align: 'right', width: '130px' },
      { key: 'empr',     label: 'Employer', template: this.emprCellTpl, align: 'right', width: '130px' },
      { key: 'total',    label: 'Total', template: this.totalCellTpl, align: 'right', width: '130px' },
    ];
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.service.getFiltered({
      year: this.year(),
      month: this.month(),
      pageNumber: this.pageNumber(),
      pageSize: this.pageSize(),
    }).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) {
          this.rows.set(res.data.items);
          this.totalCount.set(res.data.totalCount);
        }
      },
      error: () => this.loading.set(false),
    });
    // Also refresh the monthly summary
    this.service.getReport(this.year(), this.month()).subscribe({
      next: (res) => {
        if (res.success && res.data) this.report.set(res.data);
      },
      error: () => this.report.set(null),
    });
  }

  onAnyFilterChange(): void {
    this.pageNumber.set(1);
    this.load();
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.load();
  }

  bulkCompute(): void {
    if (this.busy()) return;
    this.confirm.confirm({
      title: 'Compute PF for period',
      message: `Compute PF contributions for all eligible employees in ${this.months[this.month() - 1].label} ${this.year()}?`,
      confirmLabel: 'Run Computation',
    }).subscribe((ok) => {
      if (!ok) return;
      this.busy.set(true);
      this.service.bulkCompute(this.year(), this.month()).subscribe({
        next: (res) => {
          this.busy.set(false);
          if (res.success && res.data) {
            const r = res.data;
            this.toast.success(`Computed: ${r.successCount} created, ${r.skippedCount} skipped, ${r.failedCount} failed.`);
            this.load();
          } else {
            this.toast.error(res.message || 'Bulk computation failed.');
          }
        },
        error: (err: HttpErrorResponse) => {
          this.busy.set(false);
          this.toast.error(err.error?.message || 'Bulk computation failed.');
        },
      });
    });
  }

  openRules(): void { this.rulesOpen.set(true); }
  closeRules(): void { this.rulesOpen.set(false); }
}

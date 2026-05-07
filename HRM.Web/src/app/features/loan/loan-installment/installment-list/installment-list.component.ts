import { CommonModule } from '@angular/common';
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
import { RouterLink } from '@angular/router';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { heroEye, heroExclamationTriangle } from '@ng-icons/heroicons/outline';

import { ConfirmService } from '../../../../core/services/confirm.service';
import { ToastService } from '../../../../core/services/toast.service';
import { AvatarComponent } from '../../../../shared/components/avatar/avatar.component';
import { DataTableComponent } from '../../../../shared/components/data-table/data-table.component';
import {
  PageState,
  TableColumn,
} from '../../../../shared/components/data-table/data-table.types';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { CurrencyBdPipe } from '../../../../shared/pipes/currency-bd.pipe';
import {
  LoanInstallmentResponse,
  LoanInstallmentStatus,
} from '../../models/loan-installment.model';
import { LoanInstallmentService } from '../../services/loan-installment.service';

@Component({
  selector: 'hrm-installment-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    NgIcon,
    PageHeaderComponent,
    DataTableComponent,
    StatusBadgeComponent,
    AvatarComponent,
    CurrencyBdPipe,
  ],
  providers: [provideIcons({ heroEye, heroExclamationTriangle })],
  templateUrl: './installment-list.component.html',
})
export class InstallmentListComponent implements OnInit {
  private readonly service = inject(LoanInstallmentService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmService);

  readonly rows = signal<LoanInstallmentResponse[]>([]);
  readonly totalCount = signal(0);
  readonly loading = signal(true);

  readonly statusFilter = signal<string>('');
  readonly month = signal<number>(new Date().getMonth() + 1);
  readonly year = signal<number>(new Date().getFullYear());
  readonly pageNumber = signal(1);
  readonly pageSize = signal(20);
  readonly busy = signal(false);

  readonly statusOptions: LoanInstallmentStatus[] = ['Pending', 'Paid', 'Skipped', 'Overdue', 'Cancelled'];
  readonly months = Array.from({ length: 12 }, (_, i) => ({
    value: i + 1,
    label: new Date(2000, i, 1).toLocaleDateString('en-GB', { month: 'long' }),
  }));

  readonly pageState = computed<PageState>(() => ({
    pageNumber: this.pageNumber(),
    pageSize: this.pageSize(),
    totalCount: this.totalCount(),
  }));

  @ViewChild('employeeCellTpl', { static: true }) employeeCellTpl!: TemplateRef<{ $implicit: LoanInstallmentResponse }>;
  @ViewChild('amountCellTpl',   { static: true }) amountCellTpl!:   TemplateRef<{ $implicit: LoanInstallmentResponse }>;
  @ViewChild('paidCellTpl',     { static: true }) paidCellTpl!:     TemplateRef<{ $implicit: LoanInstallmentResponse }>;
  @ViewChild('statusCellTpl',   { static: true }) statusCellTpl!:   TemplateRef<{ $implicit: LoanInstallmentResponse }>;
  @ViewChild('actionsTpl',      { static: true }) actionsTpl!:      TemplateRef<{ $implicit: LoanInstallmentResponse }>;

  columns: TableColumn<LoanInstallmentResponse>[] = [];

  ngOnInit(): void {
    this.columns = [
      { key: 'loanNo',     label: 'Loan #', width: '120px' },
      { key: 'employee',   label: 'Employee', template: this.employeeCellTpl },
      { key: 'period',     label: 'Period', width: '140px' },
      { key: 'inst',       label: 'Inst #', width: '80px', align: 'center' },
      { key: 'amount',     label: 'Amount', template: this.amountCellTpl, align: 'right', width: '120px' },
      { key: 'paid',       label: 'Paid', template: this.paidCellTpl, align: 'right', width: '120px' },
      { key: 'status',     label: 'Status', template: this.statusCellTpl, align: 'center', width: '120px' },
    ];
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.service.getAll({
      status: this.statusFilter() || undefined,
      dueMonth: this.month(),
      dueYear: this.year(),
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
  }

  onAnyFilterChange(): void {
    this.pageNumber.set(1);
    this.load();
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.load();
  }

  markOverdue(): void {
    if (this.busy()) return;
    this.confirm.confirm({
      title: 'Mark overdue',
      message: `Flag all unpaid installments due in ${this.year()}-${String(this.month()).padStart(2, '0')} as overdue?`,
      confirmLabel: 'Mark Overdue',
    }).subscribe((ok) => {
      if (!ok) return;
      this.busy.set(true);
      this.service.markOverdue(this.year(), this.month()).subscribe({
        next: (res) => {
          this.busy.set(false);
          if (res.success) {
            const count = res.data?.updated ?? 0;
            this.toast.success(`${count} installment(s) marked as overdue.`);
            this.load();
          } else {
            this.toast.error(res.message || 'Failed.');
          }
        },
        error: (err) => {
          this.busy.set(false);
          this.toast.error(err.error?.message || 'Failed.');
        },
      });
    });
  }
}

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
import { heroEye, heroBanknotes } from '@ng-icons/heroicons/outline';

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
  EmployeeLoanResponse,
  EmployeeLoanStatus,
} from '../../models/employee-loan.model';
import { EmployeeLoanService } from '../../services/employee-loan.service';

@Component({
  selector: 'hrm-active-loan-list',
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
  providers: [provideIcons({ heroEye, heroBanknotes })],
  templateUrl: './active-list.component.html',
  styles: [
    `
      .progress-bar {
        position: relative;
        background: #E2E8F0;
        height: 6px;
        border-radius: 3px;
        overflow: hidden;
        min-width: 80px;
      }
      .progress-bar__fill {
        background: var(--brand-primary, #2563EB);
        height: 100%;
        transition: width 200ms ease;
      }
    `,
  ],
})
export class ActiveListComponent implements OnInit {
  private readonly service = inject(EmployeeLoanService);

  readonly rows = signal<EmployeeLoanResponse[]>([]);
  readonly totalCount = signal(0);
  readonly loading = signal(true);

  readonly statusFilter = signal<string>('Active');
  readonly pageNumber = signal(1);
  readonly pageSize = signal(20);

  readonly statusOptions: EmployeeLoanStatus[] = ['Active', 'Completed', 'Defaulted', 'Cancelled'];

  readonly pageState = computed<PageState>(() => ({
    pageNumber: this.pageNumber(),
    pageSize: this.pageSize(),
    totalCount: this.totalCount(),
  }));

  @ViewChild('employeeCellTpl', { static: true }) employeeCellTpl!: TemplateRef<{ $implicit: EmployeeLoanResponse }>;
  @ViewChild('principalTpl',    { static: true }) principalTpl!:    TemplateRef<{ $implicit: EmployeeLoanResponse }>;
  @ViewChild('emiTpl',          { static: true }) emiTpl!:          TemplateRef<{ $implicit: EmployeeLoanResponse }>;
  @ViewChild('outstandingTpl',  { static: true }) outstandingTpl!:  TemplateRef<{ $implicit: EmployeeLoanResponse }>;
  @ViewChild('progressTpl',     { static: true }) progressTpl!:     TemplateRef<{ $implicit: EmployeeLoanResponse }>;
  @ViewChild('statusCellTpl',   { static: true }) statusCellTpl!:   TemplateRef<{ $implicit: EmployeeLoanResponse }>;
  @ViewChild('actionsTpl',      { static: true }) actionsTpl!:      TemplateRef<{ $implicit: EmployeeLoanResponse }>;

  columns: TableColumn<EmployeeLoanResponse>[] = [];

  ngOnInit(): void {
    this.columns = [
      { key: 'loanNo',      label: 'Loan #', width: '130px' },
      { key: 'employee',    label: 'Employee', template: this.employeeCellTpl },
      { key: 'loanType',    label: 'Type', width: '110px' },
      { key: 'principal',   label: 'Principal', template: this.principalTpl, align: 'right', width: '130px' },
      { key: 'emi',         label: 'EMI', template: this.emiTpl, align: 'right', width: '120px' },
      { key: 'outstanding', label: 'Outstanding', template: this.outstandingTpl, align: 'right', width: '140px' },
      { key: 'progress',    label: 'Progress', template: this.progressTpl, width: '160px' },
      { key: 'status',      label: 'Status', template: this.statusCellTpl, align: 'center', width: '120px' },
    ];
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.service.getAll({
      status: this.statusFilter() || undefined,
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

  onFilterChange(): void {
    this.pageNumber.set(1);
    this.load();
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.load();
  }
}

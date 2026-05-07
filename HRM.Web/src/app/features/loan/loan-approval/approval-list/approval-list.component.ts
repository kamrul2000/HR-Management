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
import { heroEye, heroCheck, heroPaperAirplane } from '@ng-icons/heroicons/outline';

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
  LoanApplicationResponse,
  LoanApplicationStatus,
} from '../../models/loan-application.model';
import { LoanApplicationService } from '../../services/loan-application.service';

@Component({
  selector: 'hrm-loan-approval-list',
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
  providers: [provideIcons({ heroEye, heroCheck, heroPaperAirplane })],
  templateUrl: './approval-list.component.html',
})
export class ApprovalListComponent implements OnInit {
  private readonly service = inject(LoanApplicationService);

  readonly rows = signal<LoanApplicationResponse[]>([]);
  readonly totalCount = signal(0);
  readonly loading = signal(true);

  readonly statusFilter = signal<LoanApplicationStatus>('Recommended');
  readonly pageNumber = signal(1);
  readonly pageSize = signal(20);

  readonly statusOptions: LoanApplicationStatus[] = ['Recommended', 'Approved'];

  readonly pageState = computed<PageState>(() => ({
    pageNumber: this.pageNumber(),
    pageSize: this.pageSize(),
    totalCount: this.totalCount(),
  }));

  @ViewChild('employeeCellTpl', { static: true }) employeeCellTpl!: TemplateRef<{ $implicit: LoanApplicationResponse }>;
  @ViewChild('amountCellTpl',   { static: true }) amountCellTpl!:   TemplateRef<{ $implicit: LoanApplicationResponse }>;
  @ViewChild('statusCellTpl',   { static: true }) statusCellTpl!:   TemplateRef<{ $implicit: LoanApplicationResponse }>;
  @ViewChild('actionsTpl',      { static: true }) actionsTpl!:      TemplateRef<{ $implicit: LoanApplicationResponse }>;

  columns: TableColumn<LoanApplicationResponse>[] = [];

  ngOnInit(): void {
    this.columns = [
      { key: 'applicationNo', label: 'Application #', width: '140px' },
      { key: 'employee',      label: 'Employee', template: this.employeeCellTpl },
      { key: 'loanType',      label: 'Type', width: '110px' },
      { key: 'amount',        label: 'Requested', template: this.amountCellTpl, align: 'right', width: '130px' },
      { key: 'tenure',        label: 'Tenure', width: '110px' },
      { key: 'status',        label: 'Status', template: this.statusCellTpl, align: 'center', width: '120px' },
    ];
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.service.getAll({
      status: this.statusFilter(),
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

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
import {
  heroPlus,
  heroEye,
  heroXMark,
  heroCheck,
  heroDocumentText,
} from '@ng-icons/heroicons/outline';

import { ConfirmService } from '../../../../core/services/confirm.service';
import { ToastService } from '../../../../core/services/toast.service';
import { AvatarComponent } from '../../../../shared/components/avatar/avatar.component';
import { DataTableComponent } from '../../../../shared/components/data-table/data-table.component';
import {
  PageState,
  TableColumn,
} from '../../../../shared/components/data-table/data-table.types';
import { DrawerComponent } from '../../../../shared/components/drawer/drawer.component';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { CurrencyBdPipe } from '../../../../shared/pipes/currency-bd.pipe';
import {
  LoanApplicationResponse,
  LoanApplicationStatus,
} from '../../models/loan-application.model';
import { LoanApplicationService } from '../../services/loan-application.service';
import { ApplicationFormComponent } from '../application-form/application-form.component';

const STATUS_OPTIONS: LoanApplicationStatus[] = [
  'Pending',
  'Recommended',
  'Approved',
  'Rejected',
  'Disbursed',
  'Cancelled',
];

@Component({
  selector: 'hrm-loan-application-list',
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
    DrawerComponent,
    ApplicationFormComponent,
  ],
  providers: [provideIcons({ heroPlus, heroEye, heroXMark, heroCheck, heroDocumentText })],
  templateUrl: './application-list.component.html',
})
export class ApplicationListComponent implements OnInit {
  private readonly service = inject(LoanApplicationService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmService);

  readonly rows = signal<LoanApplicationResponse[]>([]);
  readonly totalCount = signal(0);
  readonly loading = signal(true);

  readonly statusFilter = signal<string>('');
  readonly loanTypeFilter = signal<string>('');
  readonly pageNumber = signal(1);
  readonly pageSize = signal(20);

  readonly statusOptions = STATUS_OPTIONS;
  readonly loanTypes = ['Personal', 'Emergency', 'Salary', 'Festival', 'Education', 'Medical'];

  readonly drawerOpen = signal(false);
  readonly cancelOpen = signal<LoanApplicationResponse | null>(null);
  readonly cancelReason = signal<string>('');
  readonly busy = signal(false);

  readonly pageState = computed<PageState>(() => ({
    pageNumber: this.pageNumber(),
    pageSize: this.pageSize(),
    totalCount: this.totalCount(),
  }));

  @ViewChild('employeeCellTpl', { static: true }) employeeCellTpl!: TemplateRef<{ $implicit: LoanApplicationResponse }>;
  @ViewChild('amountCellTpl',   { static: true }) amountCellTpl!:   TemplateRef<{ $implicit: LoanApplicationResponse }>;
  @ViewChild('emiCellTpl',      { static: true }) emiCellTpl!:      TemplateRef<{ $implicit: LoanApplicationResponse }>;
  @ViewChild('typeCellTpl',     { static: true }) typeCellTpl!:     TemplateRef<{ $implicit: LoanApplicationResponse }>;
  @ViewChild('statusCellTpl',   { static: true }) statusCellTpl!:   TemplateRef<{ $implicit: LoanApplicationResponse }>;
  @ViewChild('actionsTpl',      { static: true }) actionsTpl!:      TemplateRef<{ $implicit: LoanApplicationResponse }>;

  columns: TableColumn<LoanApplicationResponse>[] = [];

  ngOnInit(): void {
    this.columns = [
      { key: 'applicationNo', label: 'Application #', width: '140px' },
      { key: 'employee',      label: 'Employee', template: this.employeeCellTpl },
      { key: 'loanType',      label: 'Type', template: this.typeCellTpl, width: '110px' },
      { key: 'amount',        label: 'Requested', template: this.amountCellTpl, align: 'right', width: '130px' },
      { key: 'tenure',        label: 'Tenure', width: '100px' },
      { key: 'emi',           label: 'Est. EMI', template: this.emiCellTpl, align: 'right', width: '130px' },
      { key: 'status',        label: 'Status', template: this.statusCellTpl, align: 'center', width: '120px' },
    ];
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.service.getAll({
      status: this.statusFilter() || undefined,
      loanType: this.loanTypeFilter() || undefined,
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

  openCreate(): void { this.drawerOpen.set(true); }
  closeCreate(): void { this.drawerOpen.set(false); }

  onCreated(): void {
    this.closeCreate();
    this.load();
  }

  openCancel(row: LoanApplicationResponse): void {
    this.cancelOpen.set(row);
    this.cancelReason.set('');
  }

  closeCancel(): void {
    this.cancelOpen.set(null);
  }

  confirmCancel(): void {
    const row = this.cancelOpen();
    if (!row || this.busy()) return;
    const reason = this.cancelReason().trim();
    if (!reason) {
      this.toast.error('Provide a cancellation reason.');
      return;
    }
    this.busy.set(true);
    this.service.cancel(row.id, { cancellationReason: reason }).subscribe({
      next: (res) => {
        this.busy.set(false);
        if (res.success && res.data) {
          this.toast.success('Application cancelled.');
          this.rows.set(this.rows().map((r) => (r.id === row.id ? res.data! : r)));
          this.closeCancel();
        } else {
          this.toast.error(res.message || 'Cancellation failed.');
        }
      },
      error: (err) => {
        this.busy.set(false);
        this.toast.error(err.error?.message || 'Cancellation failed.');
      },
    });
  }

  delete(row: LoanApplicationResponse): void {
    this.confirm.confirm({
      title: 'Delete application',
      message: `Delete application "${row.applicationNo}" for ${row.employeeFullName}?`,
      confirmLabel: 'Delete',
      danger: true,
    }).subscribe((ok) => {
      if (!ok) return;
      this.service.delete(row.id).subscribe({
        next: (res) => {
          if (res.success) {
            this.toast.success('Application deleted.');
            this.rows.set(this.rows().filter((r) => r.id !== row.id));
          } else {
            this.toast.error(res.message || 'Delete failed.');
          }
        },
        error: (err) => this.toast.error(err.error?.message || 'Delete failed.'),
      });
    });
  }
}

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
import {
  heroPlus,
  heroCheck,
  heroXMark,
  heroMagnifyingGlass,
  heroBanknotes,
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
import { BonusResponse, BonusStatus } from '../../models/bonus.model';
import { BonusService } from '../../services/bonus.service';
import { BonusFormComponent } from '../bonus-form/bonus-form.component';

const STATUS_OPTIONS: BonusStatus[] = ['Pending', 'Approved', 'Rejected', 'Disbursed', 'Cancelled'];

type Approval = { kind: 'closed' } | { kind: 'approve'; row: BonusResponse } | { kind: 'reject'; row: BonusResponse };

@Component({
  selector: 'hrm-bonus-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NgIcon,
    PageHeaderComponent,
    DataTableComponent,
    StatusBadgeComponent,
    AvatarComponent,
    CurrencyBdPipe,
    DrawerComponent,
    BonusFormComponent,
  ],
  providers: [provideIcons({ heroPlus, heroCheck, heroXMark, heroMagnifyingGlass, heroBanknotes })],
  templateUrl: './bonus-list.component.html',
  styleUrl: './bonus-list.component.scss',
})
export class BonusListComponent implements OnInit {
  private readonly service = inject(BonusService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmService);

  readonly rows = signal<BonusResponse[]>([]);
  readonly totalCount = signal(0);
  readonly loading = signal(true);

  readonly statusFilter = signal<string>('');
  readonly month = signal<number>(new Date().getMonth() + 1);
  readonly year = signal<number>(new Date().getFullYear());
  readonly pageNumber = signal(1);
  readonly pageSize = signal(20);

  readonly statusOptions = STATUS_OPTIONS;
  readonly months = Array.from({ length: 12 }, (_, i) => ({
    value: i + 1,
    label: new Date(2000, i, 1).toLocaleDateString('en-GB', { month: 'long' }),
  }));

  readonly drawerOpen = signal(false);
  readonly approval = signal<Approval>({ kind: 'closed' });
  readonly finalAmount = signal<number | null>(null);
  readonly remarks = signal<string>('');
  readonly busy = signal(false);

  readonly pageState = computed<PageState>(() => ({
    pageNumber: this.pageNumber(),
    pageSize: this.pageSize(),
    totalCount: this.totalCount(),
  }));

  @ViewChild('employeeCellTpl', { static: true }) employeeCellTpl!: TemplateRef<{ $implicit: BonusResponse }>;
  @ViewChild('amountCellTpl', { static: true })   amountCellTpl!:   TemplateRef<{ $implicit: BonusResponse }>;
  @ViewChild('finalCellTpl', { static: true })    finalCellTpl!:    TemplateRef<{ $implicit: BonusResponse }>;
  @ViewChild('periodCellTpl', { static: true })   periodCellTpl!:   TemplateRef<{ $implicit: BonusResponse }>;
  @ViewChild('statusCellTpl', { static: true })   statusCellTpl!:   TemplateRef<{ $implicit: BonusResponse }>;

  columns: TableColumn<BonusResponse>[] = [];

  ngOnInit(): void {
    this.columns = [
      { key: 'employee',   label: 'Employee', template: this.employeeCellTpl },
      { key: 'bonusType',  label: 'Type', width: '110px' },
      { key: 'bonusTitle', label: 'Title' },
      { key: 'amount',     label: 'Computed', template: this.amountCellTpl, align: 'right', width: '130px' },
      { key: 'final',      label: 'Final', template: this.finalCellTpl, align: 'right', width: '130px' },
      { key: 'period',     label: 'Period', template: this.periodCellTpl, width: '120px' },
      { key: 'status',     label: 'Status', template: this.statusCellTpl, align: 'center', width: '120px' },
    ];
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.service.getAll({
      status: this.statusFilter() || undefined,
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

  openApprove(row: BonusResponse): void {
    this.approval.set({ kind: 'approve', row });
    this.finalAmount.set(row.computedAmount);
    this.remarks.set('');
  }

  openReject(row: BonusResponse): void {
    this.approval.set({ kind: 'reject', row });
    this.remarks.set('');
  }

  closeApproval(): void { this.approval.set({ kind: 'closed' }); }

  approvalOpen = computed(() => this.approval().kind !== 'closed');
  approvalKind = computed(() => this.approval().kind);
  approvalRow = computed<BonusResponse | null>(() => {
    const a = this.approval();
    return a.kind === 'closed' ? null : a.row;
  });

  confirmApprove(): void {
    const a = this.approval();
    if (a.kind !== 'approve' || this.busy()) return;
    this.busy.set(true);
    this.service.approve(a.row.id, {
      finalAmount: this.finalAmount(),
      approvalRemarks: this.remarks().trim() || null,
    }).subscribe({
      next: (res) => this.handleResult(res, 'Bonus approved.'),
      error: (err: HttpErrorResponse) => this.handleError(err),
    });
  }

  confirmReject(): void {
    const a = this.approval();
    if (a.kind !== 'reject' || this.busy()) return;
    if (!this.remarks().trim()) {
      this.toast.error('Provide a reason before rejecting.');
      return;
    }
    this.busy.set(true);
    this.service.reject(a.row.id, this.remarks().trim()).subscribe({
      next: (res) => this.handleResult(res, 'Bonus rejected.'),
      error: (err: HttpErrorResponse) => this.handleError(err),
    });
  }

  cancel(row: BonusResponse): void {
    this.confirm.confirm({
      title: 'Cancel bonus',
      message: `Cancel "${row.bonusTitle}" for ${row.employeeFullName}?`,
      confirmLabel: 'Cancel bonus',
      danger: true,
    }).subscribe((ok) => {
      if (!ok) return;
      this.service.cancel(row.id).subscribe({
        next: (res) => {
          if (res.success && res.data) {
            this.toast.success('Bonus cancelled.');
            this.rows.set(this.rows().map((r) => (r.id === row.id ? res.data! : r)));
          } else {
            this.toast.error(res.message || 'Failed.');
          }
        },
        error: (err) => this.toast.error(err.error?.message || 'Failed.'),
      });
    });
  }

  private handleResult(
    res: { success: boolean; message: string; data: BonusResponse | null },
    msg: string,
  ): void {
    this.busy.set(false);
    if (res.success && res.data) {
      this.toast.success(msg);
      const updated = res.data;
      this.rows.set(this.rows().map((r) => (r.id === updated.id ? updated : r)));
      this.closeApproval();
    } else {
      this.toast.error(res.message || 'Action failed.');
    }
  }

  private handleError(err: HttpErrorResponse): void {
    this.busy.set(false);
    this.toast.error(err.error?.message || 'Action failed.');
  }

  periodLabel(row: BonusResponse): string {
    const m = new Date(row.disbursementYear, row.disbursementMonth - 1, 1);
    return m.toLocaleDateString('en-GB', { month: 'short', year: 'numeric' });
  }
}

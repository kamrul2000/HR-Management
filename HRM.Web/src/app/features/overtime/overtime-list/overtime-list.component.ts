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
import { RouterLink } from '@angular/router';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  heroPlus,
  heroCheck,
  heroXMark,
  heroChartBar,
  heroMagnifyingGlass,
} from '@ng-icons/heroicons/outline';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';

import { ToastService } from '../../../core/services/toast.service';
import { AvatarComponent } from '../../../shared/components/avatar/avatar.component';
import { DataTableComponent } from '../../../shared/components/data-table/data-table.component';
import {
  PageState,
  TableColumn,
} from '../../../shared/components/data-table/data-table.types';
import { DrawerComponent } from '../../../shared/components/drawer/drawer.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { BranchResponse } from '../../organization/models/branch.model';
import { BranchService } from '../../organization/services/branch.service';
import {
  OvertimeFilter,
  OvertimeResponse,
  OvertimeStatus,
  OvertimeType,
} from '../models/overtime.model';
import { OvertimeService } from '../services/overtime.service';

const STATUS_OPTIONS: OvertimeStatus[] = ['Pending', 'Approved', 'Rejected'];
const TYPE_OPTIONS: OvertimeType[] = ['Regular', 'Holiday', 'WeeklyOff'];

type ApproveMode = { kind: 'closed' } | { kind: 'approve'; row: OvertimeResponse } | { kind: 'reject'; row: OvertimeResponse };

@Component({
  selector: 'hrm-overtime-list',
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
    DrawerComponent,
  ],
  providers: [
    provideIcons({
      heroPlus,
      heroCheck,
      heroXMark,
      heroChartBar,
      heroMagnifyingGlass,
    }),
  ],
  templateUrl: './overtime-list.component.html',
  styleUrl: './overtime-list.component.scss',
})
export class OvertimeListComponent implements OnInit {
  private readonly service = inject(OvertimeService);
  private readonly branches = inject(BranchService);
  private readonly toast = inject(ToastService);

  readonly rows = signal<OvertimeResponse[]>([]);
  readonly totalCount = signal(0);
  readonly loading = signal(true);

  readonly searchTerm = signal('');
  readonly branchFilter = signal<number | null>(null);
  readonly statusFilter = signal<string>('');
  readonly typeFilter = signal<string>('');
  readonly month = signal<number>(new Date().getMonth() + 1);
  readonly year = signal<number>(new Date().getFullYear());
  readonly pageNumber = signal(1);
  readonly pageSize = signal(20);

  readonly statusOptions = STATUS_OPTIONS;
  readonly typeOptions = TYPE_OPTIONS;
  readonly months = Array.from({ length: 12 }, (_, i) => ({
    value: i + 1,
    label: new Date(2000, i, 1).toLocaleDateString('en-GB', { month: 'long' }),
  }));
  readonly branchOptions = signal<BranchResponse[]>([]);

  readonly drawer = signal<ApproveMode>({ kind: 'closed' });
  readonly approvedMinutes = signal(0);
  readonly remarks = signal('');
  readonly busy = signal(false);

  readonly pageState = computed<PageState>(() => ({
    pageNumber: this.pageNumber(),
    pageSize: this.pageSize(),
    totalCount: this.totalCount(),
  }));

  @ViewChild('employeeCellTpl', { static: true }) employeeCellTpl!: TemplateRef<{ $implicit: OvertimeResponse }>;
  @ViewChild('typeCellTpl', { static: true })     typeCellTpl!:     TemplateRef<{ $implicit: OvertimeResponse }>;
  @ViewChild('minutesCellTpl', { static: true })  minutesCellTpl!:  TemplateRef<{ $implicit: OvertimeResponse }>;
  @ViewChild('approvedCellTpl', { static: true }) approvedCellTpl!: TemplateRef<{ $implicit: OvertimeResponse }>;
  @ViewChild('statusCellTpl', { static: true })   statusCellTpl!:   TemplateRef<{ $implicit: OvertimeResponse }>;

  columns: TableColumn<OvertimeResponse>[] = [];

  private readonly search$ = new Subject<string>();

  ngOnInit(): void {
    this.columns = [
      { key: 'employee',     label: 'Employee', template: this.employeeCellTpl },
      { key: 'overtimeDate', label: 'Date', width: '120px' },
      { key: 'overtimeType', label: 'Type', template: this.typeCellTpl, width: '120px' },
      { key: 'requested',    label: 'Requested', template: this.minutesCellTpl, align: 'right', width: '110px' },
      { key: 'approved',     label: 'Approved',  template: this.approvedCellTpl, align: 'right', width: '110px' },
      { key: 'status',       label: 'Status', template: this.statusCellTpl, align: 'center', width: '120px' },
    ];

    this.branches.getAll({ pageSize: 200, isActive: true }).subscribe({
      next: (res) => {
        if (res.success && res.data) this.branchOptions.set(res.data.items);
      },
    });

    this.load();

    this.search$.pipe(debounceTime(300), distinctUntilChanged()).subscribe((term) => {
      this.searchTerm.set(term);
      this.pageNumber.set(1);
      this.load();
    });
  }

  load(): void {
    this.loading.set(true);
    const filter: OvertimeFilter = {
      search: this.searchTerm() || undefined,
      branchId: this.branchFilter() ?? undefined,
      status: this.statusFilter() || undefined,
      overtimeType: this.typeFilter() || undefined,
      year: this.year(),
      month: this.month(),
      pageNumber: this.pageNumber(),
      pageSize: this.pageSize(),
    };
    this.service.getAll(filter).subscribe({
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

  onSearchChange(value: string): void { this.search$.next(value); }

  onAnyFilterChange(): void {
    this.pageNumber.set(1);
    this.load();
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.load();
  }

  // ── approve / reject drawer ───────────────────────────
  openApprove(row: OvertimeResponse): void {
    this.drawer.set({ kind: 'approve', row });
    this.approvedMinutes.set(row.requestedMinutes);
    this.remarks.set('');
  }

  openReject(row: OvertimeResponse): void {
    this.drawer.set({ kind: 'reject', row });
    this.remarks.set('');
  }

  closeDrawer(): void { this.drawer.set({ kind: 'closed' }); }

  drawerOpen = computed(() => this.drawer().kind !== 'closed');
  drawerKind = computed(() => this.drawer().kind);
  drawerRow = computed<OvertimeResponse | null>(() => {
    const d = this.drawer();
    return d.kind === 'closed' ? null : d.row;
  });

  confirmApprove(): void {
    const d = this.drawer();
    if (d.kind !== 'approve' || this.busy()) return;
    this.busy.set(true);
    this.service.approve(d.row.id, {
      approvedMinutes: this.approvedMinutes(),
      approvalRemarks: this.remarks().trim() || null,
    }).subscribe({
      next: (res) => this.handleResult(res, 'Overtime approved.'),
      error: (err: HttpErrorResponse) => this.handleError(err),
    });
  }

  confirmReject(): void {
    const d = this.drawer();
    if (d.kind !== 'reject' || this.busy()) return;
    if (!this.remarks().trim()) {
      this.toast.error('Provide a reason before rejecting.');
      return;
    }
    this.busy.set(true);
    this.service.reject(d.row.id, { approvalRemarks: this.remarks().trim() }).subscribe({
      next: (res) => this.handleResult(res, 'Overtime rejected.'),
      error: (err: HttpErrorResponse) => this.handleError(err),
    });
  }

  private handleResult(
    res: { success: boolean; message: string; data: OvertimeResponse | null },
    successMsg: string,
  ): void {
    this.busy.set(false);
    if (res.success && res.data) {
      this.toast.success(successMsg);
      const updated = res.data;
      this.rows.set(this.rows().map((r) => (r.id === updated.id ? updated : r)));
      this.closeDrawer();
    } else {
      this.toast.error(res.message || 'Action failed.');
    }
  }

  private handleError(err: HttpErrorResponse): void {
    this.busy.set(false);
    this.toast.error(err.error?.message || 'Action failed.');
  }

  formatMinutes(minutes: number): string {
    if (!minutes) return '0m';
    const h = Math.floor(minutes / 60);
    const m = minutes % 60;
    if (h && m) return `${h}h ${m}m`;
    if (h) return `${h}h`;
    return `${m}m`;
  }

  formatDate(value: string): string {
    if (!value) return '—';
    return value.length >= 10 ? value.slice(0, 10) : value;
  }
}

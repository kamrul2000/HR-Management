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
  heroMagnifyingGlass,
  heroChartBar,
} from '@ng-icons/heroicons/outline';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';

import { AvatarComponent } from '../../../../shared/components/avatar/avatar.component';
import { DataTableComponent } from '../../../../shared/components/data-table/data-table.component';
import {
  PageState,
  TableColumn,
} from '../../../../shared/components/data-table/data-table.types';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { BranchResponse } from '../../../organization/models/branch.model';
import { DepartmentResponse } from '../../../organization/models/department.model';
import { BranchService } from '../../../organization/services/branch.service';
import { DepartmentService } from '../../../organization/services/department.service';
import {
  AttendanceFilter,
  AttendanceResponse,
  AttendanceStatus,
} from '../../models/attendance.model';
import { AttendanceService } from '../../services/attendance.service';

const STATUS_OPTIONS: AttendanceStatus[] = [
  'Present',
  'Absent',
  'Late',
  'HalfDay',
  'Holiday',
  'WeeklyOff',
  'OnLeave',
];

@Component({
  selector: 'hrm-attendance-list',
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
  ],
  providers: [provideIcons({ heroPlus, heroMagnifyingGlass, heroChartBar })],
  templateUrl: './attendance-list.component.html',
  styleUrl: './attendance-list.component.scss',
})
export class AttendanceListComponent implements OnInit {
  private readonly service = inject(AttendanceService);
  private readonly branches = inject(BranchService);
  private readonly departments = inject(DepartmentService);

  readonly rows = signal<AttendanceResponse[]>([]);
  readonly totalCount = signal(0);
  readonly loading = signal(true);

  readonly searchTerm = signal('');
  readonly branchFilter = signal<number | null>(null);
  readonly departmentFilter = signal<number | null>(null);
  readonly statusFilter = signal<string>('');
  readonly fromDate = signal<string>(monthStart());
  readonly toDate = signal<string>(today());
  readonly pageNumber = signal(1);
  readonly pageSize = signal(20);

  readonly statusOptions = STATUS_OPTIONS;
  readonly branchOptions = signal<BranchResponse[]>([]);
  readonly departmentOptions = signal<DepartmentResponse[]>([]);

  readonly pageState = computed<PageState>(() => ({
    pageNumber: this.pageNumber(),
    pageSize: this.pageSize(),
    totalCount: this.totalCount(),
  }));

  @ViewChild('employeeCellTpl', { static: true })
  employeeCellTpl!: TemplateRef<{ $implicit: AttendanceResponse }>;

  @ViewChild('punchesCellTpl', { static: true })
  punchesCellTpl!: TemplateRef<{ $implicit: AttendanceResponse }>;

  @ViewChild('statusCellTpl', { static: true })
  statusCellTpl!: TemplateRef<{ $implicit: AttendanceResponse }>;

  @ViewChild('lateCellTpl', { static: true })
  lateCellTpl!: TemplateRef<{ $implicit: AttendanceResponse }>;

  @ViewChild('hoursCellTpl', { static: true })
  hoursCellTpl!: TemplateRef<{ $implicit: AttendanceResponse }>;

  columns: TableColumn<AttendanceResponse>[] = [];

  private readonly search$ = new Subject<string>();

  ngOnInit(): void {
    this.columns = [
      { key: 'employee',  label: 'Employee', template: this.employeeCellTpl },
      { key: 'date',      label: 'Date', width: '130px' },
      { key: 'dutySlot',  label: 'Duty Slot' },
      { key: 'punches',   label: 'In / Out', template: this.punchesCellTpl },
      { key: 'status',    label: 'Status', template: this.statusCellTpl, align: 'center', width: '110px' },
      { key: 'late',      label: 'Late', template: this.lateCellTpl, align: 'center', width: '90px' },
      { key: 'hours',     label: 'Working', template: this.hoursCellTpl, align: 'right', width: '110px' },
    ];

    this.loadFilters();
    this.load();

    this.search$.pipe(debounceTime(300), distinctUntilChanged()).subscribe((term) => {
      this.searchTerm.set(term);
      this.pageNumber.set(1);
      this.load();
    });
  }

  loadFilters(): void {
    this.branches.getAll({ pageSize: 200 }).subscribe({
      next: (res) => {
        if (res.success && res.data) this.branchOptions.set(res.data.items);
      },
    });
    this.departments.getAll({ pageSize: 200 }).subscribe({
      next: (res) => {
        if (res.success && res.data) this.departmentOptions.set(res.data.items);
      },
    });
  }

  load(): void {
    this.loading.set(true);
    const filter: AttendanceFilter = {
      search: this.searchTerm() || undefined,
      branchId: this.branchFilter() ?? undefined,
      departmentId: this.departmentFilter() ?? undefined,
      status: this.statusFilter() || undefined,
      fromDate: this.fromDate() || undefined,
      toDate: this.toDate() || undefined,
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

  formatHours(minutes: number): string {
    if (!minutes) return '0h';
    const h = Math.floor(minutes / 60);
    const m = minutes % 60;
    return m > 0 ? `${h}h ${m}m` : `${h}h`;
  }

  formatDate(value: string): string {
    if (!value) return '—';
    return value.length >= 10 ? value.slice(0, 10) : value;
  }
}

function today(): string {
  return new Date().toISOString().slice(0, 10);
}

function monthStart(): string {
  const d = new Date();
  d.setDate(1);
  return d.toISOString().slice(0, 10);
}

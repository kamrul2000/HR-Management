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
import { Router, RouterLink } from '@angular/router';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  heroPlus,
  heroMagnifyingGlass,
  heroPencilSquare,
  heroTrash,
  heroEye,
  heroArrowDownTray,
} from '@ng-icons/heroicons/outline';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';

import { ConfirmService } from '../../../core/services/confirm.service';
import { ToastService } from '../../../core/services/toast.service';
import { AvatarComponent } from '../../../shared/components/avatar/avatar.component';
import { DataTableComponent } from '../../../shared/components/data-table/data-table.component';
import {
  PageState,
  TableColumn,
} from '../../../shared/components/data-table/data-table.types';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { BranchResponse } from '../../organization/models/branch.model';
import { DepartmentResponse } from '../../organization/models/department.model';
import { BranchService } from '../../organization/services/branch.service';
import { DepartmentService } from '../../organization/services/department.service';
import {
  EmployeeFilter,
  EmployeeResponse,
  EmployeeStatus,
} from '../models/employee.model';
import { EmployeeService } from '../services/employee.service';

const STATUS_OPTIONS: EmployeeStatus[] = [
  'Active',
  'Resigned',
  'Terminated',
  'Retired',
  'Inactive',
];

@Component({
  selector: 'hrm-employee-list',
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
  providers: [
    provideIcons({
      heroPlus,
      heroMagnifyingGlass,
      heroPencilSquare,
      heroTrash,
      heroEye,
      heroArrowDownTray,
    }),
  ],
  templateUrl: './employee-list.component.html',
  styleUrl: './employee-list.component.scss',
})
export class EmployeeListComponent implements OnInit {
  private readonly service = inject(EmployeeService);
  private readonly branches = inject(BranchService);
  private readonly departments = inject(DepartmentService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmService);
  private readonly router = inject(Router);

  readonly rows = signal<EmployeeResponse[]>([]);
  readonly totalCount = signal(0);
  readonly loading = signal(true);

  readonly searchTerm = signal('');
  readonly branchFilter = signal<number | null>(null);
  readonly departmentFilter = signal<number | null>(null);
  readonly statusFilter = signal<string>('');
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
  employeeCellTpl!: TemplateRef<{ $implicit: EmployeeResponse }>;

  @ViewChild('orgCellTpl', { static: true })
  orgCellTpl!: TemplateRef<{ $implicit: EmployeeResponse }>;

  @ViewChild('statusCellTpl', { static: true })
  statusCellTpl!: TemplateRef<{ $implicit: EmployeeResponse }>;

  columns: TableColumn<EmployeeResponse>[] = [];

  // Debounce search input so we don't hit the API on every keystroke.
  private readonly search$ = new Subject<string>();

  ngOnInit(): void {
    this.columns = [
      { key: 'employee',       label: 'Employee', template: this.employeeCellTpl },
      { key: 'designation',    label: 'Designation' },
      { key: 'org',            label: 'Branch / Department', template: this.orgCellTpl },
      { key: 'employmentType', label: 'Type', width: '140px' },
      { key: 'status',         label: 'Status', template: this.statusCellTpl, align: 'center', width: '120px' },
    ];

    this.loadFilters();
    this.load();

    this.search$
      .pipe(debounceTime(300), distinctUntilChanged())
      .subscribe((term) => {
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
    const filter: EmployeeFilter = {
      search: this.searchTerm() || undefined,
      branchId: this.branchFilter() ?? undefined,
      departmentId: this.departmentFilter() ?? undefined,
      status: this.statusFilter() || undefined,
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

  onBranchChange(value: string | number | null): void {
    this.branchFilter.set(value ? +value : null);
    this.pageNumber.set(1);
    this.load();
  }

  onDepartmentChange(value: string | number | null): void {
    this.departmentFilter.set(value ? +value : null);
    this.pageNumber.set(1);
    this.load();
  }

  onStatusChange(value: string): void {
    this.statusFilter.set(value || '');
    this.pageNumber.set(1);
    this.load();
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.load();
  }

  exportCsv(): void {
    this.toast.info('CSV export will be available soon.');
  }

  delete(employee: EmployeeResponse): void {
    this.confirm
      .confirm({
        title: 'Delete employee',
        message: `Delete "${employee.fullName}" (${employee.employeeCode})? This cannot be undone.`,
        confirmLabel: 'Delete',
        danger: true,
      })
      .subscribe((ok) => {
        if (!ok) return;
        this.service.delete(employee.id).subscribe({
          next: (res) => {
            if (res.success) {
              this.toast.success('Employee deleted.');
              this.load();
            } else {
              this.toast.error(res.message || 'Failed to delete employee.');
            }
          },
          error: (err) => {
            this.toast.error(err.error?.message || 'Failed to delete employee.');
          },
        });
      });
  }

  view(employee: EmployeeResponse): void {
    this.router.navigate(['/employees', employee.id]);
  }

  edit(employee: EmployeeResponse): void {
    this.router.navigate(['/employees', employee.id, 'edit']);
  }
}

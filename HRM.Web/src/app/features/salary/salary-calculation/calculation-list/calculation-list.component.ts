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
  heroPlay,
  heroEye,
  heroMagnifyingGlass,
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
import { CurrencyBdPipe } from '../../../../shared/pipes/currency-bd.pipe';
import { BranchResponse } from '../../../organization/models/branch.model';
import { BranchService } from '../../../organization/services/branch.service';
import {
  SalaryCalculationFilter,
  SalaryCalculationResponse,
  SalaryCalculationStatus,
} from '../../models/salary-calculation.model';
import { SalaryCalculationService } from '../../services/salary-calculation.service';

const STATUS_OPTIONS: SalaryCalculationStatus[] = ['Draft', 'Finalized', 'Cancelled'];

@Component({
  selector: 'hrm-calculation-list',
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
  providers: [provideIcons({ heroPlay, heroEye, heroMagnifyingGlass })],
  templateUrl: './calculation-list.component.html',
  styleUrl: './calculation-list.component.scss',
})
export class CalculationListComponent implements OnInit {
  private readonly service = inject(SalaryCalculationService);
  private readonly branches = inject(BranchService);
  private readonly router = inject(Router);

  readonly rows = signal<SalaryCalculationResponse[]>([]);
  readonly totalCount = signal(0);
  readonly loading = signal(true);

  readonly searchTerm = signal('');
  readonly branchFilter = signal<number | null>(null);
  readonly statusFilter = signal<string>('');
  readonly month = signal<number>(new Date().getMonth() + 1);
  readonly year = signal<number>(new Date().getFullYear());
  readonly pageNumber = signal(1);
  readonly pageSize = signal(20);

  readonly statusOptions = STATUS_OPTIONS;
  readonly branchOptions = signal<BranchResponse[]>([]);
  readonly months = Array.from({ length: 12 }, (_, i) => ({
    value: i + 1,
    label: new Date(2000, i, 1).toLocaleDateString('en-GB', { month: 'long' }),
  }));

  readonly pageState = computed<PageState>(() => ({
    pageNumber: this.pageNumber(),
    pageSize: this.pageSize(),
    totalCount: this.totalCount(),
  }));

  @ViewChild('employeeCellTpl', { static: true }) employeeCellTpl!: TemplateRef<{ $implicit: SalaryCalculationResponse }>;
  @ViewChild('grossCellTpl', { static: true })    grossCellTpl!:    TemplateRef<{ $implicit: SalaryCalculationResponse }>;
  @ViewChild('netCellTpl', { static: true })      netCellTpl!:      TemplateRef<{ $implicit: SalaryCalculationResponse }>;
  @ViewChild('statusCellTpl', { static: true })   statusCellTpl!:   TemplateRef<{ $implicit: SalaryCalculationResponse }>;

  columns: TableColumn<SalaryCalculationResponse>[] = [];

  private readonly search$ = new Subject<string>();

  ngOnInit(): void {
    this.columns = [
      { key: 'employee',        label: 'Employee', template: this.employeeCellTpl },
      { key: 'monthLabel',      label: 'Period', width: '160px' },
      { key: 'totalWorkingDays', label: 'Days', align: 'center', width: '90px' },
      { key: 'grossSalary',     label: 'Gross',   template: this.grossCellTpl, align: 'right', width: '130px' },
      { key: 'netSalary',       label: 'Net',     template: this.netCellTpl,   align: 'right', width: '140px' },
      { key: 'status',          label: 'Status',  template: this.statusCellTpl, align: 'center', width: '110px' },
    ];

    this.branches.getAll({ pageSize: 200, isActive: true }).subscribe({
      next: (res) => {
        if (res.success && res.data) this.branchOptions.set(res.data.items);
      },
    });

    this.load();

    this.search$.pipe(debounceTime(300), distinctUntilChanged()).subscribe(() => this.load());
  }

  load(): void {
    this.loading.set(true);
    const filter: SalaryCalculationFilter = {
      branchId: this.branchFilter() ?? undefined,
      status: this.statusFilter() || undefined,
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

  onAnyFilterChange(): void {
    this.pageNumber.set(1);
    this.load();
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.load();
  }

  view(row: SalaryCalculationResponse): void {
    this.router.navigate(['/salary/calculations', row.id]);
  }
}

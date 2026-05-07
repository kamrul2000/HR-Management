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
import {
  LeaveApplicationFilter,
  LeaveApplicationResponse,
  LeaveApplicationStatus,
} from '../../models/leave-application.model';
import { LeaveTypeResponse } from '../../models/leave-type.model';
import { LeaveApplicationService } from '../../services/leave-application.service';
import { LeaveTypeService } from '../../services/leave-type.service';
import { ApplicationDetailComponent } from '../application-detail/application-detail.component';

const STATUS_OPTIONS: LeaveApplicationStatus[] = ['Pending', 'Approved', 'Rejected', 'Cancelled'];

@Component({
  selector: 'hrm-application-list',
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
    ApplicationDetailComponent,
  ],
  providers: [provideIcons({ heroPlus, heroEye, heroMagnifyingGlass })],
  templateUrl: './application-list.component.html',
  styleUrl: './application-list.component.scss',
})
export class ApplicationListComponent implements OnInit {
  private readonly service = inject(LeaveApplicationService);
  private readonly leaveTypes = inject(LeaveTypeService);

  readonly rows = signal<LeaveApplicationResponse[]>([]);
  readonly totalCount = signal(0);
  readonly loading = signal(true);

  readonly searchTerm = signal('');
  readonly statusFilter = signal<string>('');
  readonly leaveTypeFilter = signal<number | null>(null);
  readonly fromDate = signal<string>('');
  readonly toDate = signal<string>('');
  readonly pageNumber = signal(1);
  readonly pageSize = signal(20);

  readonly statusOptions = STATUS_OPTIONS;
  readonly leaveTypeOptions = signal<LeaveTypeResponse[]>([]);

  readonly selected = signal<LeaveApplicationResponse | null>(null);

  readonly pageState = computed<PageState>(() => ({
    pageNumber: this.pageNumber(),
    pageSize: this.pageSize(),
    totalCount: this.totalCount(),
  }));

  @ViewChild('employeeCellTpl', { static: true }) employeeCellTpl!: TemplateRef<{ $implicit: LeaveApplicationResponse }>;
  @ViewChild('periodCellTpl', { static: true })   periodCellTpl!:   TemplateRef<{ $implicit: LeaveApplicationResponse }>;
  @ViewChild('statusCellTpl', { static: true })   statusCellTpl!:   TemplateRef<{ $implicit: LeaveApplicationResponse }>;

  columns: TableColumn<LeaveApplicationResponse>[] = [];

  private readonly search$ = new Subject<string>();

  ngOnInit(): void {
    this.columns = [
      { key: 'applicationNo',  label: 'App No', width: '130px' },
      { key: 'employee',       label: 'Employee', template: this.employeeCellTpl },
      { key: 'leaveTypeName',  label: 'Leave Type' },
      { key: 'period',         label: 'Period', template: this.periodCellTpl },
      { key: 'totalDays',      label: 'Days', align: 'right', width: '80px' },
      { key: 'status',         label: 'Status', template: this.statusCellTpl, align: 'center', width: '120px' },
    ];

    this.leaveTypes.getAll({ pageSize: 100 }).subscribe({
      next: (res) => {
        if (res.success && res.data) this.leaveTypeOptions.set(res.data.items);
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
    const filter: LeaveApplicationFilter = {
      search: this.searchTerm() || undefined,
      status: this.statusFilter() || undefined,
      leaveTypeId: this.leaveTypeFilter() ?? undefined,
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

  view(app: LeaveApplicationResponse): void {
    this.selected.set(app);
  }

  closeDetail(): void { this.selected.set(null); }

  onUpdated(updated: LeaveApplicationResponse): void {
    this.selected.set(updated);
    this.rows.set(this.rows().map((r) => (r.id === updated.id ? updated : r)));
  }
}

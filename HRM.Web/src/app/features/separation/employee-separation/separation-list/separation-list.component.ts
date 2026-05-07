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
import { heroPlus, heroEye } from '@ng-icons/heroicons/outline';

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
  SeparationResponse,
  SeparationStatus,
} from '../../models/employee-separation.model';
import { SeparationType } from '../../models/separation-reason.model';
import { EmployeeSeparationService } from '../../services/employee-separation.service';
import { SeparationFormComponent } from '../separation-form/separation-form.component';

const STATUS_OPTIONS: SeparationStatus[] = ['Pending', 'Approved', 'Processed', 'Cancelled'];
const TYPE_OPTIONS: SeparationType[] = ['Resignation', 'Termination', 'Retirement', 'Death', 'Contract End'];

@Component({
  selector: 'hrm-separation-list',
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
    SeparationFormComponent,
  ],
  providers: [provideIcons({ heroPlus, heroEye })],
  templateUrl: './separation-list.component.html',
})
export class SeparationListComponent implements OnInit {
  private readonly service = inject(EmployeeSeparationService);

  readonly rows = signal<SeparationResponse[]>([]);
  readonly totalCount = signal(0);
  readonly loading = signal(true);
  readonly drawerOpen = signal(false);

  readonly statusFilter = signal<string>('');
  readonly typeFilter = signal<string>('');
  readonly pageNumber = signal(1);
  readonly pageSize = signal(20);

  readonly statusOptions = STATUS_OPTIONS;
  readonly typeOptions = TYPE_OPTIONS;

  readonly pageState = computed<PageState>(() => ({
    pageNumber: this.pageNumber(),
    pageSize: this.pageSize(),
    totalCount: this.totalCount(),
  }));

  @ViewChild('employeeCellTpl', { static: true }) employeeCellTpl!: TemplateRef<{ $implicit: SeparationResponse }>;
  @ViewChild('typeCellTpl',     { static: true }) typeCellTpl!:     TemplateRef<{ $implicit: SeparationResponse }>;
  @ViewChild('lwdCellTpl',      { static: true }) lwdCellTpl!:      TemplateRef<{ $implicit: SeparationResponse }>;
  @ViewChild('settlementTpl',   { static: true }) settlementTpl!:   TemplateRef<{ $implicit: SeparationResponse }>;
  @ViewChild('statusCellTpl',   { static: true }) statusCellTpl!:   TemplateRef<{ $implicit: SeparationResponse }>;
  @ViewChild('actionsTpl',      { static: true }) actionsTpl!:      TemplateRef<{ $implicit: SeparationResponse }>;

  columns: TableColumn<SeparationResponse>[] = [];

  ngOnInit(): void {
    this.columns = [
      { key: 'employee',  label: 'Employee', template: this.employeeCellTpl },
      { key: 'reason',    label: 'Reason' },
      { key: 'type',      label: 'Type', template: this.typeCellTpl, width: '130px' },
      { key: 'lwd',       label: 'Last Working', template: this.lwdCellTpl, width: '140px' },
      { key: 'settlement',label: 'Settlement', template: this.settlementTpl, align: 'right', width: '140px' },
      { key: 'status',    label: 'Status', template: this.statusCellTpl, align: 'center', width: '120px' },
    ];
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.service.getAll({
      status: this.statusFilter() || undefined,
      separationType: this.typeFilter() || undefined,
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
  closeDrawer(): void { this.drawerOpen.set(false); }
  onCreated(): void {
    this.closeDrawer();
    this.load();
  }
}

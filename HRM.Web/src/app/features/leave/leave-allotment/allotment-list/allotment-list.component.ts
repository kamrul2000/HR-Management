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
  heroPencilSquare,
  heroTrash,
  heroMagnifyingGlass,
  heroSquaresPlus,
} from '@ng-icons/heroicons/outline';

import { ConfirmService } from '../../../../core/services/confirm.service';
import { ToastService } from '../../../../core/services/toast.service';
import { AvatarComponent } from '../../../../shared/components/avatar/avatar.component';
import { DataTableComponent } from '../../../../shared/components/data-table/data-table.component';
import { TableColumn } from '../../../../shared/components/data-table/data-table.types';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { LeaveAllotmentResponse } from '../../models/leave-allotment.model';
import { LeaveTypeResponse } from '../../models/leave-type.model';
import { LeaveAllotmentService } from '../../services/leave-allotment.service';
import { LeaveTypeService } from '../../services/leave-type.service';
import { AllotmentFormComponent } from '../allotment-form/allotment-form.component';

type DrawerMode = 'closed' | 'create' | { mode: 'edit'; allotment: LeaveAllotmentResponse };

@Component({
  selector: 'hrm-allotment-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    NgIcon,
    PageHeaderComponent,
    DataTableComponent,
    AvatarComponent,
    AllotmentFormComponent,
  ],
  providers: [
    provideIcons({
      heroPlus,
      heroPencilSquare,
      heroTrash,
      heroMagnifyingGlass,
      heroSquaresPlus,
    }),
  ],
  templateUrl: './allotment-list.component.html',
  styleUrl: './allotment-list.component.scss',
})
export class AllotmentListComponent implements OnInit {
  private readonly service = inject(LeaveAllotmentService);
  private readonly leaveTypes = inject(LeaveTypeService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmService);

  readonly all = signal<LeaveAllotmentResponse[]>([]);
  readonly loading = signal(true);
  readonly searchTerm = signal('');
  readonly leaveTypeFilter = signal<number | null>(null);
  readonly yearFilter = signal<number>(new Date().getFullYear());
  readonly drawer = signal<DrawerMode>('closed');

  readonly leaveTypeOptions = signal<LeaveTypeResponse[]>([]);

  @ViewChild('employeeCellTpl', { static: true })  employeeCellTpl!:  TemplateRef<{ $implicit: LeaveAllotmentResponse }>;
  @ViewChild('remainingCellTpl', { static: true }) remainingCellTpl!: TemplateRef<{ $implicit: LeaveAllotmentResponse }>;

  columns: TableColumn<LeaveAllotmentResponse>[] = [];

  readonly filtered = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    if (!term) return this.all();
    return this.all().filter((a) =>
      [a.employeeFullName ?? '', a.employeeCode ?? '', a.leaveTypeName ?? '']
        .some((v) => v.toLowerCase().includes(term)),
    );
  });

  readonly drawerOpen = computed(() => this.drawer() !== 'closed');
  readonly editing = computed<LeaveAllotmentResponse | null>(() => {
    const d = this.drawer();
    return typeof d === 'object' ? d.allotment : null;
  });

  ngOnInit(): void {
    this.columns = [
      { key: 'employee',      label: 'Employee', template: this.employeeCellTpl },
      { key: 'leaveTypeName', label: 'Leave Type' },
      { key: 'year',          label: 'Year', width: '90px', align: 'center' },
      { key: 'allocatedDays', label: 'Allocated', align: 'right', width: '100px' },
      { key: 'usedDays',      label: 'Used', align: 'right', width: '90px' },
      { key: 'remaining',     label: 'Remaining', template: this.remainingCellTpl, width: '220px' },
    ];

    this.leaveTypes.getAll({ pageSize: 100 }).subscribe({
      next: (res) => {
        if (res.success && res.data) this.leaveTypeOptions.set(res.data.items);
      },
    });

    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.service
      .getAll({
        leaveTypeId: this.leaveTypeFilter() ?? undefined,
        year: this.yearFilter(),
        pageSize: 200,
      })
      .subscribe({
        next: (res) => {
          this.loading.set(false);
          if (res.success && res.data) this.all.set(res.data.items);
        },
        error: () => this.loading.set(false),
      });
  }

  onLeaveTypeChange(value: number | null): void {
    this.leaveTypeFilter.set(value);
    this.load();
  }

  onYearChange(value: number): void {
    this.yearFilter.set(value);
    this.load();
  }

  openCreate(): void { this.drawer.set('create'); }
  openEdit(allotment: LeaveAllotmentResponse): void { this.drawer.set({ mode: 'edit', allotment }); }
  closeDrawer(): void { this.drawer.set('closed'); }

  onSaved(): void {
    this.closeDrawer();
    this.load();
  }

  delete(allotment: LeaveAllotmentResponse): void {
    this.confirm
      .confirm({
        title: 'Delete allotment',
        message: `Delete ${allotment.allocatedDays} days of "${allotment.leaveTypeName}" for ${allotment.employeeFullName}? This cannot be undone.`,
        confirmLabel: 'Delete',
        danger: true,
      })
      .subscribe((ok) => {
        if (!ok) return;
        this.service.delete(allotment.id).subscribe({
          next: (res) => {
            if (res.success) {
              this.toast.success('Allotment deleted.');
              this.all.set(this.all().filter((a) => a.id !== allotment.id));
            } else {
              this.toast.error(res.message || 'Failed to delete allotment.');
            }
          },
          error: (err) => this.toast.error(err.error?.message || 'Failed to delete allotment.'),
        });
      });
  }

  // ── helpers used by templates
  remainingPct(a: LeaveAllotmentResponse): number {
    const total = a.allocatedDays + (a.carriedForwardDays || 0);
    if (total <= 0) return 0;
    return Math.round((a.remainingDays / total) * 100);
  }

  remainingTone(a: LeaveAllotmentResponse): 'good' | 'warn' | 'low' {
    const pct = this.remainingPct(a);
    if (pct >= 50) return 'good';
    if (pct >= 20) return 'warn';
    return 'low';
  }
}

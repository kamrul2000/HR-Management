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
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  heroPencilSquare,
  heroTrash,
  heroPlus,
  heroMagnifyingGlass,
} from '@ng-icons/heroicons/outline';

import { ConfirmService } from '../../../../core/services/confirm.service';
import { ToastService } from '../../../../core/services/toast.service';
import { DataTableComponent } from '../../../../shared/components/data-table/data-table.component';
import { TableColumn } from '../../../../shared/components/data-table/data-table.types';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { BranchResponse } from '../../models/branch.model';
import { DepartmentResponse } from '../../models/department.model';
import { BranchService } from '../../services/branch.service';
import { DepartmentService } from '../../services/department.service';
import { DepartmentFormComponent } from '../department-form/department-form.component';

type DrawerMode = 'closed' | 'create' | { mode: 'edit'; department: DepartmentResponse };

@Component({
  selector: 'hrm-department-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NgIcon,
    PageHeaderComponent,
    DataTableComponent,
    StatusBadgeComponent,
    DepartmentFormComponent,
  ],
  providers: [
    provideIcons({
      heroPencilSquare,
      heroTrash,
      heroPlus,
      heroMagnifyingGlass,
    }),
  ],
  templateUrl: './department-list.component.html',
  styleUrls: ['../../company/company-list/company-list.component.scss'],
})
export class DepartmentListComponent implements OnInit {
  private readonly service = inject(DepartmentService);
  private readonly branches = inject(BranchService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmService);

  readonly all = signal<DepartmentResponse[]>([]);
  readonly loading = signal(true);
  readonly searchTerm = signal('');
  readonly branchFilter = signal<number | null>(null);
  readonly drawer = signal<DrawerMode>('closed');

  readonly branchOptions = signal<BranchResponse[]>([]);

  @ViewChild('hierarchyCellTpl', { static: true }) hierarchyCellTpl!: TemplateRef<{ $implicit: DepartmentResponse }>;
  @ViewChild('statusCellTpl', { static: true })    statusCellTpl!:    TemplateRef<{ $implicit: DepartmentResponse }>;

  columns: TableColumn<DepartmentResponse>[] = [];

  readonly filtered = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    const branchId = this.branchFilter();
    return this.all().filter((d) => {
      if (branchId && d.branchId !== branchId) return false;
      if (!term) return true;
      return [d.name, d.description ?? '', d.branchName ?? '', d.companyName ?? '']
        .some((v) => v.toLowerCase().includes(term));
    });
  });

  readonly drawerOpen = computed(() => this.drawer() !== 'closed');
  readonly editingDepartment = computed<DepartmentResponse | null>(() => {
    const d = this.drawer();
    return typeof d === 'object' ? d.department : null;
  });

  ngOnInit(): void {
    this.columns = [
      { key: 'name',        label: 'Name', sortable: true },
      { key: 'hierarchy',   label: 'Branch / Company', template: this.hierarchyCellTpl },
      { key: 'description', label: 'Description' },
      { key: 'isActive',    label: 'Status', template: this.statusCellTpl, align: 'center', width: '120px' },
    ];
    this.loadBranches();
    this.load();
  }

  loadBranches(): void {
    this.branches.getAll({ pageSize: 200 }).subscribe({
      next: (res) => {
        if (res.success && res.data) this.branchOptions.set(res.data.items);
      },
    });
  }

  load(): void {
    this.loading.set(true);
    this.service.getAll({ pageSize: 200 }).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) this.all.set(res.data.items);
      },
      error: () => this.loading.set(false),
    });
  }

  openCreate(): void { this.drawer.set('create'); }
  openEdit(department: DepartmentResponse): void { this.drawer.set({ mode: 'edit', department }); }
  closeDrawer(): void { this.drawer.set('closed'); }

  onSaved(updated: DepartmentResponse): void {
    const exists = this.all().some((d) => d.id === updated.id);
    if (exists) {
      this.all.set(this.all().map((d) => (d.id === updated.id ? updated : d)));
    } else {
      this.all.set([updated, ...this.all()]);
    }
    this.closeDrawer();
  }

  delete(department: DepartmentResponse): void {
    this.confirm
      .confirm({
        title: 'Delete department',
        message: `Are you sure you want to delete "${department.name}"? This action cannot be undone.`,
        confirmLabel: 'Delete',
        danger: true,
      })
      .subscribe((ok) => {
        if (!ok) return;
        this.service.delete(department.id).subscribe({
          next: (res) => {
            if (res.success) {
              this.toast.success('Department deleted.');
              this.all.set(this.all().filter((d) => d.id !== department.id));
            } else {
              this.toast.error(res.message || 'Failed to delete department.');
            }
          },
          error: (err) => this.toast.error(err.error?.message || 'Failed to delete department.'),
        });
      });
  }
}

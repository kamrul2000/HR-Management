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
import { DepartmentResponse } from '../../models/department.model';
import { DesignationResponse } from '../../models/designation.model';
import { DepartmentService } from '../../services/department.service';
import { DesignationService } from '../../services/designation.service';
import { DesignationFormComponent } from '../designation-form/designation-form.component';

type DrawerMode = 'closed' | 'create' | { mode: 'edit'; designation: DesignationResponse };

@Component({
  selector: 'hrm-designation-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NgIcon,
    PageHeaderComponent,
    DataTableComponent,
    StatusBadgeComponent,
    DesignationFormComponent,
  ],
  providers: [
    provideIcons({
      heroPencilSquare,
      heroTrash,
      heroPlus,
      heroMagnifyingGlass,
    }),
  ],
  templateUrl: './designation-list.component.html',
  styleUrls: ['../../company/company-list/company-list.component.scss'],
})
export class DesignationListComponent implements OnInit {
  private readonly service = inject(DesignationService);
  private readonly departments = inject(DepartmentService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmService);

  readonly all = signal<DesignationResponse[]>([]);
  readonly loading = signal(true);
  readonly searchTerm = signal('');
  readonly departmentFilter = signal<number | null>(null);
  readonly drawer = signal<DrawerMode>('closed');

  readonly departmentOptions = signal<DepartmentResponse[]>([]);

  @ViewChild('hierarchyCellTpl', { static: true }) hierarchyCellTpl!: TemplateRef<{ $implicit: DesignationResponse }>;
  @ViewChild('statusCellTpl', { static: true })    statusCellTpl!:    TemplateRef<{ $implicit: DesignationResponse }>;

  columns: TableColumn<DesignationResponse>[] = [];

  readonly filtered = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    const departmentId = this.departmentFilter();
    return this.all().filter((d) => {
      if (departmentId && d.departmentId !== departmentId) return false;
      if (!term) return true;
      return [
        d.title,
        d.grade ?? '',
        d.departmentName ?? '',
        d.branchName ?? '',
        d.companyName ?? '',
      ].some((v) => v.toLowerCase().includes(term));
    });
  });

  readonly drawerOpen = computed(() => this.drawer() !== 'closed');
  readonly editingDesignation = computed<DesignationResponse | null>(() => {
    const d = this.drawer();
    return typeof d === 'object' ? d.designation : null;
  });

  ngOnInit(): void {
    this.columns = [
      { key: 'title',     label: 'Title', sortable: true },
      { key: 'grade',     label: 'Grade', width: '120px' },
      { key: 'hierarchy', label: 'Department / Branch / Company', template: this.hierarchyCellTpl },
      { key: 'isActive',  label: 'Status', template: this.statusCellTpl, align: 'center', width: '120px' },
    ];
    this.loadDepartments();
    this.load();
  }

  loadDepartments(): void {
    this.departments.getAll({ pageSize: 200 }).subscribe({
      next: (res) => {
        if (res.success && res.data) this.departmentOptions.set(res.data.items);
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
  openEdit(designation: DesignationResponse): void { this.drawer.set({ mode: 'edit', designation }); }
  closeDrawer(): void { this.drawer.set('closed'); }

  onSaved(updated: DesignationResponse): void {
    const exists = this.all().some((d) => d.id === updated.id);
    if (exists) {
      this.all.set(this.all().map((d) => (d.id === updated.id ? updated : d)));
    } else {
      this.all.set([updated, ...this.all()]);
    }
    this.closeDrawer();
  }

  delete(designation: DesignationResponse): void {
    this.confirm
      .confirm({
        title: 'Delete designation',
        message: `Are you sure you want to delete "${designation.title}"? This action cannot be undone.`,
        confirmLabel: 'Delete',
        danger: true,
      })
      .subscribe((ok) => {
        if (!ok) return;
        this.service.delete(designation.id).subscribe({
          next: (res) => {
            if (res.success) {
              this.toast.success('Designation deleted.');
              this.all.set(this.all().filter((d) => d.id !== designation.id));
            } else {
              this.toast.error(res.message || 'Failed to delete designation.');
            }
          },
          error: (err) => this.toast.error(err.error?.message || 'Failed to delete designation.'),
        });
      });
  }
}

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
  heroFunnel,
} from '@ng-icons/heroicons/outline';

import { ConfirmService } from '../../../../core/services/confirm.service';
import { ToastService } from '../../../../core/services/toast.service';
import { DataTableComponent } from '../../../../shared/components/data-table/data-table.component';
import { TableColumn } from '../../../../shared/components/data-table/data-table.types';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { BranchResponse } from '../../models/branch.model';
import { CompanyResponse } from '../../models/company.model';
import { BranchService } from '../../services/branch.service';
import { CompanyService } from '../../services/company.service';
import { BranchFormComponent } from '../branch-form/branch-form.component';

type DrawerMode = 'closed' | 'create' | { mode: 'edit'; branch: BranchResponse };

@Component({
  selector: 'hrm-branch-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NgIcon,
    PageHeaderComponent,
    DataTableComponent,
    StatusBadgeComponent,
    BranchFormComponent,
  ],
  providers: [
    provideIcons({
      heroPencilSquare,
      heroTrash,
      heroPlus,
      heroMagnifyingGlass,
      heroFunnel,
    }),
  ],
  templateUrl: './branch-list.component.html',
  styleUrls: ['../../company/company-list/company-list.component.scss'],
})
export class BranchListComponent implements OnInit {
  private readonly service = inject(BranchService);
  private readonly companies = inject(CompanyService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmService);

  readonly all = signal<BranchResponse[]>([]);
  readonly loading = signal(true);
  readonly searchTerm = signal('');
  readonly companyFilter = signal<number | null>(null);
  readonly drawer = signal<DrawerMode>('closed');

  readonly companyOptions = signal<CompanyResponse[]>([]);

  @ViewChild('statusCellTpl', { static: true }) statusCellTpl!: TemplateRef<{ $implicit: BranchResponse }>;

  columns: TableColumn<BranchResponse>[] = [];

  readonly filtered = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    const companyId = this.companyFilter();
    return this.all().filter((b) => {
      if (companyId && b.companyId !== companyId) return false;
      if (!term) return true;
      return [b.name, b.code, b.email, b.phone, b.address ?? '', b.companyName ?? '']
        .some((v) => v.toLowerCase().includes(term));
    });
  });

  readonly drawerOpen = computed(() => this.drawer() !== 'closed');
  readonly editingBranch = computed<BranchResponse | null>(() => {
    const d = this.drawer();
    return typeof d === 'object' ? d.branch : null;
  });

  ngOnInit(): void {
    this.columns = [
      { key: 'name',        label: 'Name', sortable: true },
      { key: 'code',        label: 'Code', width: '110px' },
      { key: 'companyName', label: 'Company' },
      { key: 'address',     label: 'Address' },
      { key: 'phone',       label: 'Phone' },
      { key: 'isActive',    label: 'Status', template: this.statusCellTpl, align: 'center', width: '120px' },
    ];
    this.loadCompanies();
    this.load();
  }

  loadCompanies(): void {
    this.companies.getAll({ pageSize: 200 }).subscribe({
      next: (res) => {
        if (res.success && res.data) this.companyOptions.set(res.data.items);
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
  openEdit(branch: BranchResponse): void { this.drawer.set({ mode: 'edit', branch }); }
  closeDrawer(): void { this.drawer.set('closed'); }

  onSaved(updated: BranchResponse): void {
    const exists = this.all().some((b) => b.id === updated.id);
    if (exists) {
      this.all.set(this.all().map((b) => (b.id === updated.id ? updated : b)));
    } else {
      this.all.set([updated, ...this.all()]);
    }
    this.closeDrawer();
  }

  delete(branch: BranchResponse): void {
    this.confirm
      .confirm({
        title: 'Delete branch',
        message: `Are you sure you want to delete "${branch.name}"? This action cannot be undone.`,
        confirmLabel: 'Delete',
        danger: true,
      })
      .subscribe((ok) => {
        if (!ok) return;
        this.service.delete(branch.id).subscribe({
          next: (res) => {
            if (res.success) {
              this.toast.success('Branch deleted.');
              this.all.set(this.all().filter((b) => b.id !== branch.id));
            } else {
              this.toast.error(res.message || 'Failed to delete branch.');
            }
          },
          error: (err) => this.toast.error(err.error?.message || 'Failed to delete branch.'),
        });
      });
  }
}

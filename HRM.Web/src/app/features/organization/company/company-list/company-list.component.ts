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
  heroBuildingOffice2,
  heroMagnifyingGlass,
} from '@ng-icons/heroicons/outline';

import { ConfirmService } from '../../../../core/services/confirm.service';
import { ToastService } from '../../../../core/services/toast.service';
import { DataTableComponent } from '../../../../shared/components/data-table/data-table.component';
import { TableColumn } from '../../../../shared/components/data-table/data-table.types';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { CompanyResponse } from '../../models/company.model';
import { CompanyService } from '../../services/company.service';
import { CompanyFormComponent } from '../company-form/company-form.component';

type DrawerMode = 'closed' | 'create' | { mode: 'edit'; company: CompanyResponse };

@Component({
  selector: 'hrm-company-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NgIcon,
    PageHeaderComponent,
    DataTableComponent,
    StatusBadgeComponent,
    CompanyFormComponent,
  ],
  providers: [
    provideIcons({
      heroPencilSquare,
      heroTrash,
      heroPlus,
      heroBuildingOffice2,
      heroMagnifyingGlass,
    }),
  ],
  templateUrl: './company-list.component.html',
  styleUrl: './company-list.component.scss',
})
export class CompanyListComponent implements OnInit {
  private readonly service = inject(CompanyService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmService);

  readonly all = signal<CompanyResponse[]>([]);
  readonly loading = signal(true);
  readonly searchTerm = signal('');
  readonly drawer = signal<DrawerMode>('closed');

  // Cell templates declared at the bottom of the component template — bound at ngOnInit.
  @ViewChild('logoCellTpl', { static: true })   logoCellTpl!:   TemplateRef<{ $implicit: CompanyResponse }>;
  @ViewChild('statusCellTpl', { static: true }) statusCellTpl!: TemplateRef<{ $implicit: CompanyResponse }>;

  columns: TableColumn<CompanyResponse>[] = [];

  readonly filtered = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    if (!term) return this.all();
    return this.all().filter((c) =>
      [c.name, c.email, c.phone, c.address ?? ''].some((v) =>
        v.toLowerCase().includes(term),
      ),
    );
  });

  readonly drawerOpen = computed(() => this.drawer() !== 'closed');
  readonly editingCompany = computed<CompanyResponse | null>(() => {
    const d = this.drawer();
    return typeof d === 'object' ? d.company : null;
  });

  ngOnInit(): void {
    this.columns = [
      { key: 'logo',     label: '',       template: this.logoCellTpl,   width: '64px',  align: 'center' },
      { key: 'name',     label: 'Name',   sortable: true },
      { key: 'phone',    label: 'Phone' },
      { key: 'email',    label: 'Email' },
      { key: 'isActive', label: 'Status', template: this.statusCellTpl, width: '120px', align: 'center' },
    ];
    this.load();
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
  openEdit(company: CompanyResponse): void { this.drawer.set({ mode: 'edit', company }); }
  closeDrawer(): void { this.drawer.set('closed'); }

  onSaved(updated: CompanyResponse): void {
    const exists = this.all().some((c) => c.id === updated.id);

    if (exists) {
      this.all.set(this.all().map((c) => (c.id === updated.id ? updated : c)));
      const current = this.drawer();
      if (typeof current === 'object' && current.mode === 'edit') {
        // Refresh the open record so the logo preview updates, but leave the
        // drawer in place so the user can continue uploading or editing.
        this.drawer.set({ mode: 'edit', company: updated });
      }
    } else {
      // Created from scratch — keep the drawer open in edit mode so the user
      // can upload a logo right away.
      this.all.set([updated, ...this.all()]);
      this.drawer.set({ mode: 'edit', company: updated });
    }
  }

  delete(company: CompanyResponse): void {
    this.confirm
      .confirm({
        title: 'Delete company',
        message: `Are you sure you want to delete "${company.name}"? This action cannot be undone.`,
        confirmLabel: 'Delete',
        danger: true,
      })
      .subscribe((ok) => {
        if (!ok) return;
        this.service.delete(company.id).subscribe({
          next: (res) => {
            if (res.success) {
              this.toast.success('Company deleted.');
              this.all.set(this.all().filter((c) => c.id !== company.id));
            } else {
              this.toast.error(res.message || 'Failed to delete company.');
            }
          },
          error: (err) => this.toast.error(err.error?.message || 'Failed to delete company.'),
        });
      });
  }

  initials(name: string): string {
    if (!name) return '?';
    return name
      .trim()
      .split(/\s+/)
      .slice(0, 2)
      .map((p) => p.charAt(0).toUpperCase())
      .join('');
  }
}

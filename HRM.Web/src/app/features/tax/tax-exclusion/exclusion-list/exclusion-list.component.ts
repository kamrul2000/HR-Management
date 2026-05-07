import { CommonModule } from '@angular/common';
import {
  Component,
  OnInit,
  TemplateRef,
  ViewChild,
  inject,
  signal,
} from '@angular/core';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { heroPlus, heroPencilSquare, heroTrash } from '@ng-icons/heroicons/outline';

import { ConfirmService } from '../../../../core/services/confirm.service';
import { ToastService } from '../../../../core/services/toast.service';
import { AvatarComponent } from '../../../../shared/components/avatar/avatar.component';
import { DataTableComponent } from '../../../../shared/components/data-table/data-table.component';
import { TableColumn } from '../../../../shared/components/data-table/data-table.types';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { CurrencyBdPipe } from '../../../../shared/pipes/currency-bd.pipe';
import { TaxExclusionResponse } from '../../models/tax-exclusion.model';
import { TaxExclusionService } from '../../services/tax-exclusion.service';
import { ExclusionFormComponent } from '../exclusion-form/exclusion-form.component';

@Component({
  selector: 'hrm-tax-exclusion-list',
  standalone: true,
  imports: [
    CommonModule,
    NgIcon,
    PageHeaderComponent,
    DataTableComponent,
    StatusBadgeComponent,
    AvatarComponent,
    CurrencyBdPipe,
    ExclusionFormComponent,
  ],
  providers: [provideIcons({ heroPlus, heroPencilSquare, heroTrash })],
  templateUrl: './exclusion-list.component.html',
})
export class ExclusionListComponent implements OnInit {
  private readonly service = inject(TaxExclusionService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmService);

  readonly rows = signal<TaxExclusionResponse[]>([]);
  readonly loading = signal(true);
  readonly editing = signal<TaxExclusionResponse | null>(null);
  readonly drawerOpen = signal(false);

  @ViewChild('employeeCellTpl', { static: true }) employeeCellTpl!: TemplateRef<{ $implicit: TaxExclusionResponse }>;
  @ViewChild('typeCellTpl',     { static: true }) typeCellTpl!:     TemplateRef<{ $implicit: TaxExclusionResponse }>;
  @ViewChild('periodCellTpl',   { static: true }) periodCellTpl!:   TemplateRef<{ $implicit: TaxExclusionResponse }>;
  @ViewChild('activeTpl',       { static: true }) activeTpl!:       TemplateRef<{ $implicit: TaxExclusionResponse }>;
  @ViewChild('actionsTpl',      { static: true }) actionsTpl!:      TemplateRef<{ $implicit: TaxExclusionResponse }>;

  columns: TableColumn<TaxExclusionResponse>[] = [];

  ngOnInit(): void {
    this.columns = [
      { key: 'employee',    label: 'Employee', template: this.employeeCellTpl },
      { key: 'reason',      label: 'Reason' },
      { key: 'type',        label: 'Type', template: this.typeCellTpl, width: '160px' },
      { key: 'period',      label: 'Effective', template: this.periodCellTpl, width: '200px' },
      { key: 'certificate', label: 'Certificate', width: '140px' },
      { key: 'active',      label: 'Status', template: this.activeTpl, align: 'center', width: '120px' },
    ];
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.service.getAll().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) this.rows.set(res.data);
      },
      error: () => this.loading.set(false),
    });
  }

  openCreate(): void {
    this.editing.set(null);
    this.drawerOpen.set(true);
  }

  openEdit(row: TaxExclusionResponse): void {
    this.editing.set(row);
    this.drawerOpen.set(true);
  }

  closeDrawer(): void {
    this.drawerOpen.set(false);
    this.editing.set(null);
  }

  onSaved(): void {
    this.closeDrawer();
    this.load();
  }

  delete(row: TaxExclusionResponse): void {
    this.confirm.confirm({
      title: 'Delete tax exclusion',
      message: `Delete tax exclusion for ${row.employeeFullName}?`,
      confirmLabel: 'Delete',
      danger: true,
    }).subscribe((ok) => {
      if (!ok) return;
      this.service.delete(row.id).subscribe({
        next: (res) => {
          if (res.success) {
            this.toast.success('Exclusion deleted.');
            this.rows.set(this.rows().filter((r) => r.id !== row.id));
          } else {
            this.toast.error(res.message || 'Delete failed.');
          }
        },
        error: (err) => this.toast.error(err.error?.message || 'Delete failed.'),
      });
    });
  }
}

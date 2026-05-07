import { CommonModule } from '@angular/common';
import {
  Component,
  OnInit,
  TemplateRef,
  ViewChild,
  inject,
  signal,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { heroPlus, heroEye, heroPencilSquare, heroTrash, heroCalculator } from '@ng-icons/heroicons/outline';

import { ConfirmService } from '../../../../core/services/confirm.service';
import { ToastService } from '../../../../core/services/toast.service';
import { DataTableComponent } from '../../../../shared/components/data-table/data-table.component';
import { TableColumn } from '../../../../shared/components/data-table/data-table.types';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { CurrencyBdPipe } from '../../../../shared/pipes/currency-bd.pipe';
import { TaxSlabConfigResponse } from '../../models/tax-slab.model';
import { TaxSlabService } from '../../services/tax-slab.service';
import { TaxComputeDrawerComponent } from '../tax-compute-drawer/tax-compute-drawer.component';

@Component({
  selector: 'hrm-tax-slab-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    NgIcon,
    PageHeaderComponent,
    DataTableComponent,
    StatusBadgeComponent,
    CurrencyBdPipe,
    TaxComputeDrawerComponent,
  ],
  providers: [provideIcons({ heroPlus, heroEye, heroPencilSquare, heroTrash, heroCalculator })],
  templateUrl: './slab-list.component.html',
})
export class SlabListComponent implements OnInit {
  private readonly service = inject(TaxSlabService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmService);

  readonly rows = signal<TaxSlabConfigResponse[]>([]);
  readonly loading = signal(true);
  readonly computeOpen = signal(false);

  @ViewChild('thresholdTpl', { static: true }) thresholdTpl!: TemplateRef<{ $implicit: TaxSlabConfigResponse }>;
  @ViewChild('slabsTpl',     { static: true }) slabsTpl!:     TemplateRef<{ $implicit: TaxSlabConfigResponse }>;
  @ViewChild('activeTpl',    { static: true }) activeTpl!:    TemplateRef<{ $implicit: TaxSlabConfigResponse }>;
  @ViewChild('actionsTpl',   { static: true }) actionsTpl!:   TemplateRef<{ $implicit: TaxSlabConfigResponse }>;

  columns: TableColumn<TaxSlabConfigResponse>[] = [];

  ngOnInit(): void {
    this.columns = [
      { key: 'fiscalYear',         label: 'Fiscal Year', width: '140px' },
      { key: 'startDate',          label: 'Start' },
      { key: 'endDate',            label: 'End' },
      { key: 'taxFreeThreshold',   label: 'Tax-Free Threshold', template: this.thresholdTpl, align: 'right', width: '180px' },
      { key: 'slabCount',          label: 'Slabs', template: this.slabsTpl, align: 'center', width: '100px' },
      { key: 'isActive',           label: 'Status', template: this.activeTpl, align: 'center', width: '110px' },
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

  openCompute(): void { this.computeOpen.set(true); }
  closeCompute(): void { this.computeOpen.set(false); }

  delete(row: TaxSlabConfigResponse): void {
    this.confirm.confirm({
      title: 'Delete tax configuration',
      message: `Delete tax slab configuration for ${row.fiscalYear}? This cannot be undone.`,
      confirmLabel: 'Delete',
      danger: true,
    }).subscribe((ok) => {
      if (!ok) return;
      this.service.delete(row.id).subscribe({
        next: (res) => {
          if (res.success) {
            this.toast.success('Configuration deleted.');
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

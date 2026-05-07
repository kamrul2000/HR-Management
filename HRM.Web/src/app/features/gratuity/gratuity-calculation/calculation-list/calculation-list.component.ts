import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  OnInit,
  TemplateRef,
  ViewChild,
  inject,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { heroPlus, heroCheck, heroXMark, heroEye } from '@ng-icons/heroicons/outline';

import { ConfirmService } from '../../../../core/services/confirm.service';
import { ToastService } from '../../../../core/services/toast.service';
import { AvatarComponent } from '../../../../shared/components/avatar/avatar.component';
import { DataTableComponent } from '../../../../shared/components/data-table/data-table.component';
import { TableColumn } from '../../../../shared/components/data-table/data-table.types';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { CurrencyBdPipe } from '../../../../shared/pipes/currency-bd.pipe';
import {
  GratuityCalculationResponse,
  GratuityReport,
} from '../../models/gratuity-calculation.model';
import { GratuityCalculationService } from '../../services/gratuity-calculation.service';
import { ComputeGratuityDrawerComponent } from '../compute-gratuity-drawer/compute-gratuity-drawer.component';

@Component({
  selector: 'hrm-gratuity-calculation-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NgIcon,
    PageHeaderComponent,
    DataTableComponent,
    StatusBadgeComponent,
    AvatarComponent,
    CurrencyBdPipe,
    ComputeGratuityDrawerComponent,
  ],
  providers: [provideIcons({ heroPlus, heroCheck, heroXMark, heroEye })],
  templateUrl: './calculation-list.component.html',
  styles: [
    `
      .stat-strip {
        display: grid;
        grid-template-columns: repeat(4, 1fr);
        gap: 12px;
        margin-bottom: 16px;
      }
      .stat-card {
        background: #fff;
        border: 1px solid #E2E8F0;
        border-radius: 8px;
        padding: 14px 16px;
      }
      .stat-card__label { font-size: 12px; color: #64748B; text-transform: uppercase; letter-spacing: 0.4px; }
      .stat-card__value { font-weight: 600; color: #0F172A; font-size: 18px; margin-top: 4px; }
      .stat-card--eligible   .stat-card__value { color: #16A34A; }
      .stat-card--ineligible .stat-card__value { color: #DC2626; }
      .stat-card--total      .stat-card__value { color: #2563EB; }
    `,
  ],
})
export class CalculationListComponent implements OnInit {
  private readonly service = inject(GratuityCalculationService);
  private readonly toast = inject(ToastService);
  private readonly confirm = inject(ConfirmService);

  readonly rows = signal<GratuityCalculationResponse[]>([]);
  readonly report = signal<GratuityReport | null>(null);
  readonly loading = signal(true);
  readonly drawerOpen = signal(false);

  @ViewChild('employeeCellTpl', { static: true }) employeeCellTpl!: TemplateRef<{ $implicit: GratuityCalculationResponse }>;
  @ViewChild('serviceTpl',      { static: true }) serviceTpl!:      TemplateRef<{ $implicit: GratuityCalculationResponse }>;
  @ViewChild('amountTpl',       { static: true }) amountTpl!:       TemplateRef<{ $implicit: GratuityCalculationResponse }>;
  @ViewChild('statusCellTpl',   { static: true }) statusCellTpl!:   TemplateRef<{ $implicit: GratuityCalculationResponse }>;
  @ViewChild('eligibleTpl',     { static: true }) eligibleTpl!:     TemplateRef<{ $implicit: GratuityCalculationResponse }>;
  @ViewChild('actionsTpl',      { static: true }) actionsTpl!:      TemplateRef<{ $implicit: GratuityCalculationResponse }>;

  columns: TableColumn<GratuityCalculationResponse>[] = [];

  ngOnInit(): void {
    this.columns = [
      { key: 'employee',    label: 'Employee', template: this.employeeCellTpl },
      { key: 'separation',  label: 'Separation', width: '140px' },
      { key: 'service',     label: 'Service', template: this.serviceTpl, width: '140px' },
      { key: 'rule',        label: 'Rule', width: '160px' },
      { key: 'amount',      label: 'Gratuity', template: this.amountTpl, align: 'right', width: '140px' },
      { key: 'eligible',    label: 'Eligible', template: this.eligibleTpl, align: 'center', width: '100px' },
      { key: 'status',      label: 'Status', template: this.statusCellTpl, align: 'center', width: '120px' },
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
    this.service.getReport().subscribe({
      next: (res) => {
        if (res.success && res.data) this.report.set(res.data);
      },
    });
  }

  openCreate(): void { this.drawerOpen.set(true); }
  closeDrawer(): void { this.drawerOpen.set(false); }
  onSaved(): void {
    this.closeDrawer();
    this.load();
  }

  finalize(row: GratuityCalculationResponse): void {
    this.confirm.confirm({
      title: 'Finalize gratuity',
      message: `Finalize the gratuity calculation for ${row.employeeFullName}? Once finalized, this record cannot be edited.`,
      confirmLabel: 'Finalize',
    }).subscribe((ok) => {
      if (!ok) return;
      this.service.finalize(row.id).subscribe({
        next: (res) => {
          if (res.success && res.data) {
            this.toast.success('Gratuity finalized.');
            this.rows.set(this.rows().map((r) => (r.id === row.id ? res.data! : r)));
          } else {
            this.toast.error(res.message || 'Failed.');
          }
        },
        error: (err: HttpErrorResponse) => this.toast.error(err.error?.message || 'Failed.'),
      });
    });
  }

  cancel(row: GratuityCalculationResponse): void {
    const reason = window.prompt('Reason for cancellation:');
    if (!reason || !reason.trim()) return;
    this.service.cancel(row.id, reason.trim()).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.toast.success('Calculation cancelled.');
          this.rows.set(this.rows().map((r) => (r.id === row.id ? res.data! : r)));
        } else {
          this.toast.error(res.message || 'Failed.');
        }
      },
      error: (err: HttpErrorResponse) => this.toast.error(err.error?.message || 'Failed.'),
    });
  }
}

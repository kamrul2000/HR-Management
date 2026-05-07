import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  EventEmitter,
  Output,
  inject,
  signal,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { ToastService } from '../../../../core/services/toast.service';
import { CurrencyBdPipe } from '../../../../shared/pipes/currency-bd.pipe';
import { DrawerComponent } from '../../../../shared/components/drawer/drawer.component';
import { TaxComputationResult } from '../../models/tax-slab.model';
import { TaxSlabService } from '../../services/tax-slab.service';

@Component({
  selector: 'hrm-tax-compute-drawer',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DrawerComponent, CurrencyBdPipe],
  templateUrl: './tax-compute-drawer.component.html',
  styles: [
    `
      .summary-grid {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 12px;
        margin-bottom: 16px;
      }
      .summary-card {
        background: #F8FAFC;
        border: 1px solid #E2E8F0;
        border-radius: 8px;
        padding: 12px 14px;
      }
      .summary-card--primary { background: #ECFDF5; border-color: #A7F3D0; }
      .summary-card__label { font-size: 12px; color: #64748B; text-transform: uppercase; letter-spacing: 0.4px; }
      .summary-card__value { font-weight: 600; color: #0F172A; font-size: 18px; margin-top: 4px; }
      .summary-card--primary .summary-card__label { color: #047857; }
      .summary-card--primary .summary-card__value { color: #064E3B; }

      .breakdown { width: 100%; border-collapse: collapse; font-size: 13px; }
      .breakdown th { text-align: left; padding: 6px 8px; background: #F1F5F9; font-size: 11px; text-transform: uppercase; color: #64748B; }
      .breakdown td { padding: 8px; border-bottom: 1px solid #F1F5F9; }
      .breakdown .text-right { text-align: right; }
    `,
  ],
})
export class TaxComputeDrawerComponent {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(TaxSlabService);
  private readonly toast = inject(ToastService);

  @Output() dismiss = new EventEmitter<void>();

  readonly form = this.fb.nonNullable.group({
    annualIncome: [0, [Validators.required, Validators.min(0)]],
    fiscalYear: [''],
  });

  readonly computing = signal(false);
  readonly result = signal<TaxComputationResult | null>(null);

  compute(): void {
    if (this.form.invalid || this.computing()) {
      this.form.markAllAsTouched();
      return;
    }
    const raw = this.form.getRawValue();
    this.computing.set(true);
    this.service.compute({
      annualIncome: +raw.annualIncome,
      fiscalYear: raw.fiscalYear?.trim() || undefined,
    }).subscribe({
      next: (res) => {
        this.computing.set(false);
        if (res.success && res.data) this.result.set(res.data);
        else this.toast.error(res.message || 'Failed to compute.');
      },
      error: (err: HttpErrorResponse) => {
        this.computing.set(false);
        this.toast.error(err.error?.message || 'Failed to compute.');
      },
    });
  }

  reset(): void {
    this.form.reset({ annualIncome: 0, fiscalYear: '' });
    this.result.set(null);
  }
}

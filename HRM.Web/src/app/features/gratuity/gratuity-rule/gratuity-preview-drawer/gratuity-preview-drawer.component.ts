import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  EventEmitter,
  Output,
  inject,
  signal,
} from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';

import { ToastService } from '../../../../core/services/toast.service';
import { CurrencyBdPipe } from '../../../../shared/pipes/currency-bd.pipe';
import { DrawerComponent } from '../../../../shared/components/drawer/drawer.component';
import { EmployeeResponse } from '../../../employee/models/employee.model';
import { EmployeeService } from '../../../employee/services/employee.service';
import { GratuityPreviewResult } from '../../models/gratuity-rule.model';
import { GratuityRuleService } from '../../services/gratuity-rule.service';

@Component({
  selector: 'hrm-gratuity-preview-drawer',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, DrawerComponent, CurrencyBdPipe],
  templateUrl: './gratuity-preview-drawer.component.html',
  styles: [
    `
      .emp-autocomplete {
        position: absolute;
        z-index: 5;
        background: #fff;
        border: 1px solid #E2E8F0;
        border-radius: 8px;
        box-shadow: 0 4px 6px rgba(0,0,0,.07);
        margin-top: 4px;
        width: 100%;
        max-height: 240px;
        overflow-y: auto;
        list-style: none;
        padding: 4px;
        font-size: 13px;
      }
      .emp-autocomplete li { padding: 8px 10px; border-radius: 4px; cursor: pointer; }
      .emp-autocomplete li:hover { background: #F1F5F9; }

      .ineligible {
        background: #FEF2F2;
        border: 1px solid #FECACA;
        color: #991B1B;
        padding: 12px 14px;
        border-radius: 8px;
        margin-top: 12px;
      }
      .summary {
        background: #ECFDF5;
        border: 1px solid #A7F3D0;
        border-radius: 8px;
        padding: 14px 16px;
        margin: 16px 0;
        text-align: center;
      }
      .summary__label { font-size: 12px; color: #047857; text-transform: uppercase; letter-spacing: 0.4px; }
      .summary__value { font-weight: 700; color: #064E3B; font-size: 28px; margin-top: 4px; }
      .summary__cap   { font-size: 11px; color: #92400E; margin-top: 6px; }

      .grid { display: grid; grid-template-columns: 1fr 1fr; gap: 8px 16px; font-size: 13px; }
      .grid__label { color: #64748B; font-size: 11px; text-transform: uppercase; letter-spacing: 0.4px; }
      .grid__value { font-weight: 600; color: #0F172A; }
    `,
  ],
})
export class GratuityPreviewDrawerComponent {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(GratuityRuleService);
  private readonly employees = inject(EmployeeService);
  private readonly toast = inject(ToastService);

  @Output() dismiss = new EventEmitter<void>();

  readonly form = this.fb.nonNullable.group({
    employeeId: [0, [Validators.required, Validators.min(1)]],
    employeeLabel: [''],
    separationDate: [new Date().toISOString().slice(0, 10), [Validators.required]],
  });

  readonly running = signal(false);
  readonly result = signal<GratuityPreviewResult | null>(null);
  readonly searchResults = signal<EmployeeResponse[]>([]);
  private readonly search$ = new Subject<string>();

  constructor() {
    this.search$.pipe(debounceTime(300), distinctUntilChanged()).subscribe((term) => this.runSearch(term));
  }

  onEmployeeSearch(term: string): void {
    this.form.patchValue({ employeeLabel: term, employeeId: 0 });
    this.search$.next(term.trim());
  }

  private runSearch(term: string): void {
    if (!term) { this.searchResults.set([]); return; }
    this.employees.getAll({ search: term, pageSize: 8 }).subscribe({
      next: (res) => {
        if (res.success && res.data) this.searchResults.set(res.data.items);
      },
    });
  }

  pickEmployee(emp: EmployeeResponse): void {
    this.form.patchValue({
      employeeId: emp.id,
      employeeLabel: `${emp.fullName} (${emp.employeeCode})`,
    });
    this.searchResults.set([]);
  }

  preview(): void {
    if (this.form.invalid || this.running()) {
      this.form.markAllAsTouched();
      return;
    }
    const raw = this.form.getRawValue();
    this.running.set(true);
    this.service.preview({
      employeeId: raw.employeeId,
      separationDate: raw.separationDate,
    }).subscribe({
      next: (res) => {
        this.running.set(false);
        if (res.success && res.data) this.result.set(res.data);
        else this.toast.error(res.message || 'Preview failed.');
      },
      error: (err: HttpErrorResponse) => {
        this.running.set(false);
        this.toast.error(err.error?.message || 'Preview failed.');
      },
    });
  }
}

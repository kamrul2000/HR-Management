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
import { DrawerComponent } from '../../../../shared/components/drawer/drawer.component';
import { EmployeeResponse } from '../../../employee/models/employee.model';
import { EmployeeService } from '../../../employee/services/employee.service';
import { ComputeGratuityDto } from '../../models/gratuity-calculation.model';
import { GratuityCalculationService } from '../../services/gratuity-calculation.service';

@Component({
  selector: 'hrm-compute-gratuity-drawer',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, DrawerComponent],
  templateUrl: './compute-gratuity-drawer.component.html',
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
    `,
  ],
})
export class ComputeGratuityDrawerComponent {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(GratuityCalculationService);
  private readonly employees = inject(EmployeeService);
  private readonly toast = inject(ToastService);

  @Output() saved = new EventEmitter<void>();
  @Output() dismiss = new EventEmitter<void>();

  readonly form = this.fb.nonNullable.group({
    employeeId: [0, [Validators.required, Validators.min(1)]],
    employeeLabel: [''],
    separationDate: [new Date().toISOString().slice(0, 10), [Validators.required]],
    remarks: [''],
  });

  readonly saving = signal(false);
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

  submit(): void {
    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }
    const raw = this.form.getRawValue();
    const dto: ComputeGratuityDto = {
      employeeId: raw.employeeId,
      separationDate: raw.separationDate,
      remarks: raw.remarks?.trim() || null,
    };
    this.saving.set(true);
    this.service.compute(dto).subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success) {
          this.toast.success('Gratuity computed.');
          this.saved.emit();
        } else {
          this.toast.error(res.message || 'Compute failed.');
        }
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(err.error?.message || 'Compute failed.');
      },
    });
  }
}

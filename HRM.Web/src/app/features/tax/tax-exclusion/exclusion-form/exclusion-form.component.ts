import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  EventEmitter,
  Input,
  OnInit,
  Output,
  computed,
  inject,
  signal,
} from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';

import { ToastService } from '../../../../core/services/toast.service';
import { DrawerComponent } from '../../../../shared/components/drawer/drawer.component';
import { EmployeeResponse } from '../../../employee/models/employee.model';
import { EmployeeService } from '../../../employee/services/employee.service';
import {
  CreateTaxExclusionDto,
  TaxExclusionResponse,
  TaxExclusionType,
  UpdateTaxExclusionDto,
} from '../../models/tax-exclusion.model';
import { TaxExclusionService } from '../../services/tax-exclusion.service';

@Component({
  selector: 'hrm-tax-exclusion-form',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, DrawerComponent],
  templateUrl: './exclusion-form.component.html',
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
export class ExclusionFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(TaxExclusionService);
  private readonly employees = inject(EmployeeService);
  private readonly toast = inject(ToastService);

  @Input() editing: TaxExclusionResponse | null = null;
  @Output() saved = new EventEmitter<void>();
  @Output() dismiss = new EventEmitter<void>();

  readonly form = this.fb.nonNullable.group({
    employeeId: [0, [Validators.required, Validators.min(1)]],
    employeeLabel: [''],
    reason: ['', [Validators.required, Validators.maxLength(500)]],
    exclusionType: ['Full' as TaxExclusionType, [Validators.required]],
    partialExclusionAmount: [null as number | null],
    effectiveFrom: [new Date().toISOString().slice(0, 10), [Validators.required]],
    effectiveTo: [null as string | null],
    certificateNo: [''],
    isActive: [true],
  });

  readonly saving = signal(false);
  readonly searchResults = signal<EmployeeResponse[]>([]);
  private readonly search$ = new Subject<string>();

  readonly isPartial = computed(() => this.form.value.exclusionType === 'Partial');
  readonly isEdit = computed(() => !!this.editing);

  ngOnInit(): void {
    this.search$.pipe(debounceTime(300), distinctUntilChanged()).subscribe((term) => this.runSearch(term));

    if (this.editing) {
      const e = this.editing;
      this.form.patchValue({
        employeeId: e.employeeId,
        employeeLabel: `${e.employeeFullName} (${e.employeeCode})`,
        reason: e.reason,
        exclusionType: e.exclusionType,
        partialExclusionAmount: e.partialExclusionAmount ?? null,
        effectiveFrom: e.effectiveFrom?.slice(0, 10) || '',
        effectiveTo: e.effectiveTo?.slice(0, 10) || null,
        certificateNo: e.certificateNo ?? '',
        isActive: e.isActive,
      });
      this.form.controls.employeeId.disable();
      this.form.controls.employeeLabel.disable();
      this.form.controls.exclusionType.disable();
      this.form.controls.effectiveFrom.disable();
    }
  }

  onEmployeeSearch(term: string): void {
    this.form.patchValue({ employeeLabel: term, employeeId: 0 });
    this.search$.next(term.trim());
  }

  private runSearch(term: string): void {
    if (!term) { this.searchResults.set([]); return; }
    this.employees.getAll({ search: term, status: 'Active', pageSize: 8 }).subscribe({
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
    if (this.saving()) return;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const raw = this.form.getRawValue();
    if (raw.exclusionType === 'Partial' && (!raw.partialExclusionAmount || raw.partialExclusionAmount <= 0)) {
      this.toast.error('Provide a partial exclusion amount.');
      return;
    }

    this.saving.set(true);

    if (this.editing) {
      const dto: UpdateTaxExclusionDto = {
        reason: raw.reason.trim(),
        partialExclusionAmount: raw.exclusionType === 'Partial' ? +raw.partialExclusionAmount! : null,
        effectiveTo: raw.effectiveTo || null,
        certificateNo: raw.certificateNo?.trim() || null,
        isActive: raw.isActive,
      };
      this.service.update(this.editing.id, dto).subscribe({
        next: (res) => this.afterSave(res, 'updated'),
        error: (err: HttpErrorResponse) => this.afterError(err),
      });
    } else {
      const dto: CreateTaxExclusionDto = {
        employeeId: raw.employeeId,
        reason: raw.reason.trim(),
        exclusionType: raw.exclusionType,
        partialExclusionAmount: raw.exclusionType === 'Partial' ? +raw.partialExclusionAmount! : null,
        effectiveFrom: raw.effectiveFrom,
        effectiveTo: raw.effectiveTo || null,
        certificateNo: raw.certificateNo?.trim() || null,
      };
      this.service.create(dto).subscribe({
        next: (res) => this.afterSave(res, 'created'),
        error: (err: HttpErrorResponse) => this.afterError(err),
      });
    }
  }

  private afterSave(res: { success: boolean; message: string }, what: string): void {
    this.saving.set(false);
    if (res.success) {
      this.toast.success(`Exclusion ${what}.`);
      this.saved.emit();
    } else {
      this.toast.error(res.message || 'Failed to save.');
    }
  }

  private afterError(err: HttpErrorResponse): void {
    this.saving.set(false);
    this.toast.error(err.error?.message || 'Failed to save.');
  }
}

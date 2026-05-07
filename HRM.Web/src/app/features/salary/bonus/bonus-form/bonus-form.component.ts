import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  EventEmitter,
  Input,
  OnChanges,
  OnInit,
  Output,
  SimpleChanges,
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
  BonusBasis,
  CreateBonusDto,
} from '../../models/bonus.model';
import { BonusService } from '../../services/bonus.service';

const BASIS_OPTIONS: { value: BonusBasis; label: string }[] = [
  { value: 'Fixed',              label: 'Fixed amount' },
  { value: 'PercentageOfBasic',  label: '% of Basic Salary' },
  { value: 'PercentageOfGross',  label: '% of Gross Salary' },
];

@Component({
  selector: 'hrm-bonus-form',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, DrawerComponent],
  templateUrl: './bonus-form.component.html',
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
      .emp-autocomplete li {
        padding: 8px 10px;
        border-radius: 4px;
        cursor: pointer;
      }
      .emp-autocomplete li:hover { background: #F1F5F9; }
    `,
  ],
})
export class BonusFormComponent implements OnInit, OnChanges {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(BonusService);
  private readonly employees = inject(EmployeeService);
  private readonly toast = inject(ToastService);

  @Output() saved = new EventEmitter<void>();
  @Output() dismiss = new EventEmitter<void>();

  readonly form = this.fb.nonNullable.group({
    employeeId: [0, [Validators.required, Validators.min(1)]],
    employeeLabel: [''],
    bonusType: ['Festival', [Validators.required, Validators.maxLength(50)]],
    bonusTitle: ['', [Validators.required, Validators.maxLength(150)]],
    calculationBasis: ['Fixed' as BonusBasis, [Validators.required]],
    fixedAmount: [null as number | null],
    basisPercentage: [null as number | null],
    disbursementMonth: [new Date().getMonth() + 1, [Validators.required, Validators.min(1), Validators.max(12)]],
    disbursementYear: [new Date().getFullYear(), [Validators.required, Validators.min(2000), Validators.max(2100)]],
    isDisbursedWithSalary: [true],
    remarks: [''],
  });

  readonly saving = signal(false);
  readonly searchResults = signal<EmployeeResponse[]>([]);
  readonly basisOptions = BASIS_OPTIONS;
  private readonly search$ = new Subject<string>();

  readonly months = Array.from({ length: 12 }, (_, i) => ({
    value: i + 1,
    label: new Date(2000, i, 1).toLocaleDateString('en-GB', { month: 'long' }),
  }));

  readonly isFixed = computed(() => this.form.value.calculationBasis === 'Fixed');

  ngOnInit(): void {
    this.search$.pipe(debounceTime(300), distinctUntilChanged()).subscribe((term) => this.runSearch(term));
  }

  ngOnChanges(_: SimpleChanges): void {/* no-op (component is single-purpose for create) */}

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
    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }
    const raw = this.form.getRawValue();

    const dto: CreateBonusDto = {
      employeeId: raw.employeeId,
      bonusType: raw.bonusType.trim(),
      bonusTitle: raw.bonusTitle.trim(),
      calculationBasis: raw.calculationBasis,
      fixedAmount: raw.calculationBasis === 'Fixed' ? raw.fixedAmount : null,
      basisPercentage: raw.calculationBasis !== 'Fixed' ? raw.basisPercentage : null,
      disbursementMonth: raw.disbursementMonth,
      disbursementYear: raw.disbursementYear,
      isDisbursedWithSalary: raw.isDisbursedWithSalary,
      remarks: raw.remarks?.trim() || null,
    };

    this.saving.set(true);
    this.service.create(dto).subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success) {
          this.toast.success('Bonus created.');
          this.saved.emit();
        } else {
          this.toast.error(res.message || 'Failed to save bonus.');
        }
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(err.error?.message || 'Failed to save bonus.');
      },
    });
  }

  hasError(field: keyof typeof this.form.controls, error: string): boolean {
    const ctrl = this.form.controls[field];
    return ctrl.touched && ctrl.hasError(error);
  }
}

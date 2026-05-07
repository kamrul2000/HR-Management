import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  EventEmitter,
  OnInit,
  Output,
  computed,
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
import { CreateLoanApplicationDto } from '../../models/loan-application.model';
import { LoanApplicationService } from '../../services/loan-application.service';

const LOAN_TYPES = ['Personal', 'Emergency', 'Salary', 'Festival', 'Education', 'Medical'];

@Component({
  selector: 'hrm-loan-application-form',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, DrawerComponent, CurrencyBdPipe],
  templateUrl: './application-form.component.html',
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

      .emi-pill {
        display: flex;
        justify-content: space-between;
        background: #F0F9FF;
        border: 1px solid #BAE6FD;
        border-radius: 8px;
        padding: 12px 14px;
        margin-top: 8px;
      }
      .emi-pill__label { font-size: 12px; color: #0369A1; text-transform: uppercase; letter-spacing: 0.4px; }
      .emi-pill__value { font-weight: 600; color: #0C4A6E; }
    `,
  ],
})
export class ApplicationFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(LoanApplicationService);
  private readonly employees = inject(EmployeeService);
  private readonly toast = inject(ToastService);

  @Output() saved = new EventEmitter<void>();
  @Output() dismiss = new EventEmitter<void>();

  readonly loanTypes = LOAN_TYPES;

  readonly form = this.fb.nonNullable.group({
    employeeId: [0, [Validators.required, Validators.min(1)]],
    employeeLabel: [''],
    loanType: ['Personal', [Validators.required, Validators.maxLength(50)]],
    requestedAmount: [0, [Validators.required, Validators.min(1)]],
    requestedTenureMonths: [12, [Validators.required, Validators.min(1), Validators.max(120)]],
    purpose: ['', [Validators.required, Validators.maxLength(1000)]],
  });

  readonly saving = signal(false);
  readonly searchResults = signal<EmployeeResponse[]>([]);
  private readonly search$ = new Subject<string>();

  readonly amountSig = signal<number>(0);
  readonly tenureSig = signal<number>(12);

  /** Plain straight-line installment estimate, no interest. */
  readonly estimatedEmi = computed(() => {
    const amt = +this.amountSig() || 0;
    const ten = +this.tenureSig() || 0;
    if (amt <= 0 || ten <= 0) return 0;
    return amt / ten;
  });

  ngOnInit(): void {
    this.search$.pipe(debounceTime(300), distinctUntilChanged()).subscribe((term) => this.runSearch(term));
    this.form.controls.requestedAmount.valueChanges.subscribe((v) => this.amountSig.set(+(v ?? 0)));
    this.form.controls.requestedTenureMonths.valueChanges.subscribe((v) => this.tenureSig.set(+(v ?? 0)));
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
    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }
    const raw = this.form.getRawValue();
    const dto: CreateLoanApplicationDto = {
      employeeId: raw.employeeId,
      loanType: raw.loanType,
      requestedAmount: +raw.requestedAmount,
      requestedTenureMonths: +raw.requestedTenureMonths,
      purpose: raw.purpose.trim(),
    };
    this.saving.set(true);
    this.service.create(dto).subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success) {
          this.toast.success('Application submitted.');
          this.saved.emit();
        } else {
          this.toast.error(res.message || 'Failed to submit.');
        }
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(err.error?.message || 'Failed to submit.');
      },
    });
  }

  hasError(field: keyof typeof this.form.controls, error: string): boolean {
    const ctrl = this.form.controls[field];
    return ctrl.touched && ctrl.hasError(error);
  }
}

import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  EventEmitter,
  Input,
  OnInit,
  Output,
  inject,
  signal,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { ToastService } from '../../../../core/services/toast.service';
import { CurrencyBdPipe } from '../../../../shared/pipes/currency-bd.pipe';
import { DrawerComponent } from '../../../../shared/components/drawer/drawer.component';
import { CreateEmployeeLoanDto } from '../../models/employee-loan.model';
import { LoanApplicationResponse } from '../../models/loan-application.model';
import { LoanApprovalResponse } from '../../models/loan-approval.model';
import { EmployeeLoanService } from '../../services/employee-loan.service';

@Component({
  selector: 'hrm-loan-disburse-drawer',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DrawerComponent, CurrencyBdPipe],
  templateUrl: './disburse-drawer.component.html',
  styles: [
    `
      .ref-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
      .ref-card {
        background: #F8FAFC;
        border: 1px solid #E2E8F0;
        border-radius: 8px;
        padding: 10px 12px;
      }
      .ref-card__label { font-size: 12px; color: #64748B; text-transform: uppercase; letter-spacing: 0.4px; }
      .ref-card__value { font-weight: 600; color: #0F172A; margin-top: 2px; }
    `,
  ],
})
export class DisburseDrawerComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(EmployeeLoanService);
  private readonly toast = inject(ToastService);

  @Input({ required: true }) application!: LoanApplicationResponse;
  @Input() approval: LoanApprovalResponse | null = null;
  @Output() saved = new EventEmitter<number>();
  @Output() dismiss = new EventEmitter<void>();

  readonly form = this.fb.nonNullable.group({
    disbursementDate: [new Date().toISOString().slice(0, 10), [Validators.required]],
    firstInstallmentMonth: [new Date().getMonth() + 2 > 12 ? 1 : new Date().getMonth() + 2, [Validators.required]],
    firstInstallmentYear: [new Date().getMonth() + 2 > 12 ? new Date().getFullYear() + 1 : new Date().getFullYear(), [Validators.required]],
    remarks: [''],
  });

  readonly months = Array.from({ length: 12 }, (_, i) => ({
    value: i + 1,
    label: new Date(2000, i, 1).toLocaleDateString('en-GB', { month: 'long' }),
  }));

  readonly saving = signal(false);

  ngOnInit(): void {/* defaults set in form group */}

  submit(): void {
    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }
    const raw = this.form.getRawValue();
    const dto: CreateEmployeeLoanDto = {
      loanApplicationId: this.application.id,
      disbursementDate: raw.disbursementDate,
      firstInstallmentMonth: +raw.firstInstallmentMonth,
      firstInstallmentYear: +raw.firstInstallmentYear,
      remarks: raw.remarks?.trim() || null,
    };

    this.saving.set(true);
    this.service.create(dto).subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success && res.data) {
          this.toast.success('Loan disbursed.');
          this.saved.emit(res.data.id);
        } else {
          this.toast.error(res.message || 'Disbursement failed.');
        }
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(err.error?.message || 'Disbursement failed.');
      },
    });
  }
}

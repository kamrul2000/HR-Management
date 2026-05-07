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
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { ToastService } from '../../../../core/services/toast.service';
import { CurrencyBdPipe } from '../../../../shared/pipes/currency-bd.pipe';
import { DrawerComponent } from '../../../../shared/components/drawer/drawer.component';
import { LoanApplicationResponse } from '../../models/loan-application.model';
import {
  CreateLoanApprovalDto,
  InterestType,
} from '../../models/loan-approval.model';
import { LoanRecommendationResponse } from '../../models/loan-recommendation.model';
import { LoanApprovalService } from '../../services/loan-approval.service';

@Component({
  selector: 'hrm-loan-approval-drawer',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DrawerComponent, CurrencyBdPipe],
  templateUrl: './approval-drawer.component.html',
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

      .preview {
        background: #ECFDF5;
        border: 1px solid #A7F3D0;
        border-radius: 8px;
        padding: 12px 14px;
        margin-top: 12px;
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 8px 16px;
      }
      .preview__label { font-size: 12px; color: #047857; text-transform: uppercase; letter-spacing: 0.4px; }
      .preview__value { font-weight: 600; color: #064E3B; }
    `,
  ],
})
export class ApprovalDrawerComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(LoanApprovalService);
  private readonly toast = inject(ToastService);

  @Input({ required: true }) application!: LoanApplicationResponse;
  @Input() recommendation: LoanRecommendationResponse | null = null;
  @Output() saved = new EventEmitter<void>();
  @Output() dismiss = new EventEmitter<void>();

  readonly form = this.fb.nonNullable.group({
    decision: ['Approve' as 'Approve' | 'Reject', [Validators.required]],
    approvedAmount: [0, [Validators.min(0)]],
    approvedTenureMonths: [12, [Validators.min(0), Validators.max(120)]],
    interestRate: [0, [Validators.min(0), Validators.max(100)]],
    interestType: ['Flat' as InterestType, [Validators.required]],
    remarks: ['', [Validators.required, Validators.maxLength(1000)]],
  });

  readonly saving = signal(false);
  readonly isApprove = computed(() => this.form.value.decision === 'Approve');

  /** Live signals so computed re-runs on field changes. */
  readonly amountSig = signal<number>(0);
  readonly tenureSig = signal<number>(12);
  readonly rateSig = signal<number>(0);
  readonly typeSig = signal<InterestType>('Flat');

  readonly preview = computed(() => {
    const principal = +this.amountSig() || 0;
    const months = +this.tenureSig() || 0;
    const rate = +this.rateSig() || 0;
    const type = this.typeSig();
    if (principal <= 0 || months <= 0) return { emi: 0, total: 0, interest: 0 };

    if (type === 'None' || rate <= 0) {
      const emi = principal / months;
      return { emi, total: principal, interest: 0 };
    }

    if (type === 'Flat') {
      const totalInterest = principal * (rate / 100) * (months / 12);
      const total = principal + totalInterest;
      return { emi: total / months, total, interest: totalInterest };
    }

    // Reducing balance
    const r = rate / 12 / 100;
    if (r === 0) {
      const emi = principal / months;
      return { emi, total: principal, interest: 0 };
    }
    const factor = Math.pow(1 + r, months);
    const emi = (principal * r * factor) / (factor - 1);
    const total = emi * months;
    return { emi, total, interest: total - principal };
  });

  ngOnInit(): void {
    const recAmount = this.recommendation?.recommendedAmount ?? this.application.requestedAmount;
    const recTenure = this.recommendation?.recommendedTenureMonths ?? this.application.requestedTenureMonths;
    this.form.patchValue({
      approvedAmount: recAmount,
      approvedTenureMonths: recTenure,
      interestRate: 0,
      interestType: 'Flat',
    });
    this.amountSig.set(+recAmount);
    this.tenureSig.set(+recTenure);

    this.form.controls.approvedAmount.valueChanges.subscribe((v) => this.amountSig.set(+(v ?? 0)));
    this.form.controls.approvedTenureMonths.valueChanges.subscribe((v) => this.tenureSig.set(+(v ?? 0)));
    this.form.controls.interestRate.valueChanges.subscribe((v) => this.rateSig.set(+(v ?? 0)));
    this.form.controls.interestType.valueChanges.subscribe((v) => this.typeSig.set(v ?? 'Flat'));
  }

  setDecision(d: 'Approve' | 'Reject'): void {
    this.form.patchValue({ decision: d });
  }

  submit(): void {
    if (this.saving()) return;
    const raw = this.form.getRawValue();
    if (!raw.remarks.trim()) {
      this.toast.error('Provide remarks.');
      return;
    }
    if (raw.decision === 'Approve' && (!raw.approvedAmount || !raw.approvedTenureMonths)) {
      this.toast.error('Approved amount and tenure are required.');
      return;
    }

    const dto: CreateLoanApprovalDto = {
      loanApplicationId: this.application.id,
      decision: raw.decision,
      approvedAmount: raw.decision === 'Approve' ? +raw.approvedAmount : null,
      approvedTenureMonths: raw.decision === 'Approve' ? +raw.approvedTenureMonths : null,
      interestRate: raw.decision === 'Approve' ? +raw.interestRate : null,
      interestType: raw.decision === 'Approve' ? raw.interestType : null,
      remarks: raw.remarks.trim(),
    };

    this.saving.set(true);
    this.service.process(dto).subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success) {
          this.toast.success('Approval decision recorded.');
          this.saved.emit();
        } else {
          this.toast.error(res.message || 'Failed to record decision.');
        }
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(err.error?.message || 'Failed to record decision.');
      },
    });
  }
}

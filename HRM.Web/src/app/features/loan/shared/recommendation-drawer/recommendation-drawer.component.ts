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
import { CreateRecommendationDto } from '../../models/loan-recommendation.model';
import { LoanRecommendationService } from '../../services/loan-recommendation.service';

@Component({
  selector: 'hrm-loan-recommendation-drawer',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DrawerComponent, CurrencyBdPipe],
  templateUrl: './recommendation-drawer.component.html',
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
export class RecommendationDrawerComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(LoanRecommendationService);
  private readonly toast = inject(ToastService);

  @Input({ required: true }) application!: LoanApplicationResponse;
  @Output() saved = new EventEmitter<void>();
  @Output() dismiss = new EventEmitter<void>();

  readonly form = this.fb.nonNullable.group({
    decision: ['Recommend' as 'Recommend' | 'Reject', [Validators.required]],
    recommendedAmount: [0, [Validators.min(0)]],
    recommendedTenureMonths: [0, [Validators.min(0), Validators.max(120)]],
    remarks: ['', [Validators.required, Validators.maxLength(1000)]],
  });

  readonly saving = signal(false);
  readonly isRecommend = computed(() => this.form.value.decision === 'Recommend');

  ngOnInit(): void {
    this.form.patchValue({
      recommendedAmount: this.application.requestedAmount,
      recommendedTenureMonths: this.application.requestedTenureMonths,
    });
  }

  setDecision(d: 'Recommend' | 'Reject'): void {
    this.form.patchValue({ decision: d });
  }

  submit(): void {
    if (this.saving()) return;
    const raw = this.form.getRawValue();
    if (!raw.remarks.trim()) {
      this.toast.error('Provide remarks for this decision.');
      return;
    }
    if (raw.decision === 'Recommend' && (!raw.recommendedAmount || !raw.recommendedTenureMonths)) {
      this.toast.error('Recommended amount and tenure are required.');
      return;
    }

    const dto: CreateRecommendationDto = {
      loanApplicationId: this.application.id,
      decision: raw.decision,
      recommendedAmount: raw.decision === 'Recommend' ? +raw.recommendedAmount : null,
      recommendedTenureMonths: raw.decision === 'Recommend' ? +raw.recommendedTenureMonths : null,
      remarks: raw.remarks.trim(),
    };

    this.saving.set(true);
    this.service.recommend(dto).subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success) {
          this.toast.success('Recommendation submitted.');
          this.saved.emit();
        } else {
          this.toast.error(res.message || 'Failed to submit recommendation.');
        }
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(err.error?.message || 'Failed to submit recommendation.');
      },
    });
  }
}

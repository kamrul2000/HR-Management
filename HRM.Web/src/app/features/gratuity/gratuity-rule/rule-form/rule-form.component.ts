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
import { DrawerComponent } from '../../../../shared/components/drawer/drawer.component';
import {
  CreateGratuityRuleDto,
  GratuityCalculationBasis,
  GratuityRuleResponse,
  UpdateGratuityRuleDto,
} from '../../models/gratuity-rule.model';
import { GratuityRuleService } from '../../services/gratuity-rule.service';

@Component({
  selector: 'hrm-gratuity-rule-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DrawerComponent],
  templateUrl: './rule-form.component.html',
})
export class RuleFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(GratuityRuleService);
  private readonly toast = inject(ToastService);

  @Input() editing: GratuityRuleResponse | null = null;
  @Output() saved = new EventEmitter<void>();
  @Output() dismiss = new EventEmitter<void>();

  readonly form = this.fb.nonNullable.group({
    ruleName: ['', [Validators.required, Validators.maxLength(100)]],
    minServiceYears: [5, [Validators.required, Validators.min(0.5), Validators.max(10)]],
    calculationBasis: ['Basic' as GratuityCalculationBasis, [Validators.required]],
    ratePerYear: [30, [Validators.required, Validators.min(0.01), Validators.max(60)]],
    maxGratuityAmount: [null as number | null],
    maxServiceYearsCapped: [null as number | null],
    proRataEnabled: [true],
    effectiveFrom: [new Date().toISOString().slice(0, 10), [Validators.required]],
    isActive: [true],
    description: [''],
  });

  readonly saving = signal(false);
  readonly isEdit = computed(() => !!this.editing);

  ngOnInit(): void {
    if (this.editing) {
      const r = this.editing;
      this.form.patchValue({
        ruleName: r.ruleName,
        minServiceYears: r.minServiceYears,
        calculationBasis: r.calculationBasis,
        ratePerYear: r.ratePerYear,
        maxGratuityAmount: r.maxGratuityAmount ?? null,
        maxServiceYearsCapped: r.maxServiceYearsCapped ?? null,
        proRataEnabled: r.proRataEnabled,
        effectiveFrom: r.effectiveFrom?.slice(0, 10) || '',
        isActive: r.isActive,
        description: r.description ?? '',
      });
      this.form.controls.effectiveFrom.disable();
    }
  }

  submit(): void {
    if (this.saving()) return;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const raw = this.form.getRawValue();
    this.saving.set(true);

    if (this.editing) {
      const dto: UpdateGratuityRuleDto = {
        ruleName: raw.ruleName.trim(),
        minServiceYears: +raw.minServiceYears,
        calculationBasis: raw.calculationBasis,
        ratePerYear: +raw.ratePerYear,
        maxGratuityAmount: raw.maxGratuityAmount ?? null,
        maxServiceYearsCapped: raw.maxServiceYearsCapped ?? null,
        proRataEnabled: raw.proRataEnabled,
        isActive: raw.isActive,
        description: raw.description?.trim() || null,
      };
      this.service.update(this.editing.id, dto).subscribe({
        next: (res) => this.afterSave(res, 'updated'),
        error: (err: HttpErrorResponse) => this.afterError(err),
      });
    } else {
      const dto: CreateGratuityRuleDto = {
        ruleName: raw.ruleName.trim(),
        minServiceYears: +raw.minServiceYears,
        calculationBasis: raw.calculationBasis,
        ratePerYear: +raw.ratePerYear,
        maxGratuityAmount: raw.maxGratuityAmount ?? null,
        maxServiceYearsCapped: raw.maxServiceYearsCapped ?? null,
        proRataEnabled: raw.proRataEnabled,
        effectiveFrom: raw.effectiveFrom,
        description: raw.description?.trim() || null,
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
      this.toast.success(`Gratuity rule ${what}.`);
      this.saved.emit();
    } else {
      this.toast.error(res.message || 'Save failed.');
    }
  }

  private afterError(err: HttpErrorResponse): void {
    this.saving.set(false);
    this.toast.error(err.error?.message || 'Save failed.');
  }
}

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
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { heroPlus, heroPencilSquare, heroArrowLeft, heroCheck } from '@ng-icons/heroicons/outline';

import { ToastService } from '../../../../core/services/toast.service';
import { DrawerComponent } from '../../../../shared/components/drawer/drawer.component';
import { LoadingSkeletonComponent } from '../../../../shared/components/loading-skeleton/loading-skeleton.component';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { CurrencyBdPipe } from '../../../../shared/pipes/currency-bd.pipe';
import {
  CreatePfRuleDto,
  PfRuleResponse,
  UpdatePfRuleDto,
} from '../../models/pf-contribution.model';
import { PfContributionService } from '../../services/pf-contribution.service';

@Component({
  selector: 'hrm-pf-rule-list',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    NgIcon,
    DrawerComponent,
    LoadingSkeletonComponent,
    StatusBadgeComponent,
    CurrencyBdPipe,
  ],
  providers: [provideIcons({ heroPlus, heroPencilSquare, heroArrowLeft, heroCheck })],
  templateUrl: './pf-rule-list.component.html',
  styles: [
    `
      .rule-card {
        border: 1px solid #E2E8F0;
        border-radius: 8px;
        padding: 12px 14px;
        margin-bottom: 12px;
      }
      .rule-card__head {
        display: flex;
        align-items: center;
        justify-content: space-between;
        margin-bottom: 8px;
      }
      .rule-card__head h4 { margin: 0; font-size: 14px; font-weight: 600; }
      .rule-card__grid {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 8px;
        font-size: 13px;
      }
      .rule-card__label { color: #64748B; font-size: 11px; text-transform: uppercase; letter-spacing: 0.4px; }
    `,
  ],
})
export class PfRuleListComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(PfContributionService);
  private readonly toast = inject(ToastService);

  @Output() dismiss = new EventEmitter<void>();

  readonly rules = signal<PfRuleResponse[]>([]);
  readonly loading = signal(true);
  readonly saving = signal(false);

  readonly mode = signal<'list' | 'form'>('list');
  readonly editing = signal<PfRuleResponse | null>(null);

  readonly form = this.fb.nonNullable.group({
    ruleName: ['', [Validators.required, Validators.maxLength(100)]],
    employeeContributionRate: [10, [Validators.required, Validators.min(0.01), Validators.max(100)]],
    employerContributionRate: [10, [Validators.required, Validators.min(0), Validators.max(100)]],
    pfBase: ['Basic', [Validators.required]],
    minEligibleSalary: [null as number | null],
    maxContributionAmount: [null as number | null],
    effectiveFrom: [new Date().toISOString().slice(0, 10), [Validators.required]],
    effectiveTo: [null as string | null],
    isActive: [true],
    description: [''],
  });

  readonly isEdit = computed(() => !!this.editing());
  readonly title = computed(() => {
    if (this.mode() === 'form') return this.isEdit() ? 'Edit PF Rule' : 'New PF Rule';
    return 'PF Rules';
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.service.getAllRules().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) this.rules.set(res.data);
      },
      error: () => this.loading.set(false),
    });
  }

  openCreate(): void {
    this.editing.set(null);
    this.form.reset({
      ruleName: '',
      employeeContributionRate: 10,
      employerContributionRate: 10,
      pfBase: 'Basic',
      minEligibleSalary: null,
      maxContributionAmount: null,
      effectiveFrom: new Date().toISOString().slice(0, 10),
      effectiveTo: null,
      isActive: true,
      description: '',
    });
    this.form.controls.pfBase.enable();
    this.form.controls.effectiveFrom.enable();
    this.mode.set('form');
  }

  openEdit(rule: PfRuleResponse): void {
    this.editing.set(rule);
    this.form.patchValue({
      ruleName: rule.ruleName,
      employeeContributionRate: rule.employeeContributionRate,
      employerContributionRate: rule.employerContributionRate,
      pfBase: rule.pfBase,
      minEligibleSalary: rule.minEligibleSalary ?? null,
      maxContributionAmount: rule.maxContributionAmount ?? null,
      effectiveFrom: rule.effectiveFrom?.slice(0, 10) || '',
      effectiveTo: rule.effectiveTo?.slice(0, 10) || null,
      isActive: rule.isActive,
      description: rule.description ?? '',
    });
    this.form.controls.pfBase.disable();
    this.form.controls.effectiveFrom.disable();
    this.mode.set('form');
  }

  backToList(): void {
    this.mode.set('list');
    this.editing.set(null);
  }

  submit(): void {
    if (this.saving()) return;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const raw = this.form.getRawValue();
    this.saving.set(true);

    if (this.editing()) {
      const dto: UpdatePfRuleDto = {
        ruleName: raw.ruleName.trim(),
        employeeContributionRate: +raw.employeeContributionRate,
        employerContributionRate: +raw.employerContributionRate,
        minEligibleSalary: raw.minEligibleSalary ?? null,
        maxContributionAmount: raw.maxContributionAmount ?? null,
        effectiveTo: raw.effectiveTo || null,
        isActive: raw.isActive,
        description: raw.description?.trim() || null,
      };
      this.service.updateRule(this.editing()!.id, dto).subscribe({
        next: (res) => this.afterSave(res, 'updated'),
        error: (err: HttpErrorResponse) => this.afterError(err),
      });
    } else {
      const dto: CreatePfRuleDto = {
        ruleName: raw.ruleName.trim(),
        employeeContributionRate: +raw.employeeContributionRate,
        employerContributionRate: +raw.employerContributionRate,
        pfBase: raw.pfBase,
        minEligibleSalary: raw.minEligibleSalary ?? null,
        maxContributionAmount: raw.maxContributionAmount ?? null,
        effectiveFrom: raw.effectiveFrom,
        description: raw.description?.trim() || null,
      };
      this.service.createRule(dto).subscribe({
        next: (res) => this.afterSave(res, 'created'),
        error: (err: HttpErrorResponse) => this.afterError(err),
      });
    }
  }

  private afterSave(res: { success: boolean; message: string }, what: string): void {
    this.saving.set(false);
    if (res.success) {
      this.toast.success(`Rule ${what}.`);
      this.backToList();
      this.load();
    } else {
      this.toast.error(res.message || 'Save failed.');
    }
  }

  private afterError(err: HttpErrorResponse): void {
    this.saving.set(false);
    this.toast.error(err.error?.message || 'Save failed.');
  }
}

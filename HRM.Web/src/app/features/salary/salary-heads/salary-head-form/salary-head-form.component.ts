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
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { heroExclamationTriangle } from '@ng-icons/heroicons/outline';

import { ToastService } from '../../../../core/services/toast.service';
import { DrawerComponent } from '../../../../shared/components/drawer/drawer.component';
import { CurrencyBdPipe } from '../../../../shared/pipes/currency-bd.pipe';
import {
  CalculationMethod,
  CreateSalaryHeadDto,
  SalaryHeadResponse,
  SalaryHeadType,
  UpdateSalaryHeadDto,
} from '../../models/salary-head.model';
import { SalaryHeadService } from '../../services/salary-head.service';

const SAMPLE_BASIC = 30000;

const TYPE_OPTIONS: SalaryHeadType[] = ['Earning', 'Deduction'];
const METHOD_OPTIONS: { value: CalculationMethod; label: string }[] = [
  { value: 'Fixed',              label: 'Fixed amount' },
  { value: 'PercentageOfBasic',  label: '% of Basic Salary' },
  { value: 'PercentageOfGross',  label: '% of Gross Salary' },
  { value: 'PercentageOfHead',   label: '% of another head' },
  { value: 'PercentageOfNet',    label: '% of Net Salary (advanced)' },
];

@Component({
  selector: 'hrm-salary-head-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, NgIcon, DrawerComponent, CurrencyBdPipe],
  providers: [provideIcons({ heroExclamationTriangle })],
  templateUrl: './salary-head-form.component.html',
  styleUrl: './salary-head-form.component.scss',
})
export class SalaryHeadFormComponent implements OnInit, OnChanges {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(SalaryHeadService);
  private readonly toast = inject(ToastService);

  @Input() head: SalaryHeadResponse | null = null;

  @Output() saved = new EventEmitter<SalaryHeadResponse>();
  @Output() dismiss = new EventEmitter<void>();

  readonly form = this.fb.nonNullable.group({
    headName: ['', [Validators.required, Validators.maxLength(100)]],
    headCode: ['', [Validators.required, Validators.maxLength(20)]],
    headType: ['Earning' as SalaryHeadType, [Validators.required]],
    calculationMethod: ['Fixed' as CalculationMethod, [Validators.required]],
    percentage: [null as number | null],
    baseHeadId: [null as number | null],
    isFixed: [true],
    isTaxable: [true],
    isProvidentFundApplicable: [false],
    displayOrder: [10, [Validators.required, Validators.min(1), Validators.max(999)]],
    description: ['', [Validators.maxLength(500)]],
    isActive: [true],
  });

  readonly saving = signal(false);
  readonly typeOptions = TYPE_OPTIONS;
  readonly methodOptions = METHOD_OPTIONS;
  readonly headOptions = signal<SalaryHeadResponse[]>([]);

  readonly formValue = signal(this.form.getRawValue());

  readonly needsPercentage = computed(() =>
    this.formValue().calculationMethod !== 'Fixed',
  );
  readonly needsBaseHead = computed(() =>
    this.formValue().calculationMethod === 'PercentageOfHead',
  );
  readonly showNetWarning = computed(() =>
    this.formValue().calculationMethod === 'PercentageOfNet',
  );

  readonly preview = computed(() => {
    const v = this.formValue();
    const pct = v.percentage ?? 0;
    if (v.calculationMethod === 'Fixed') return null;
    if (!pct) return null;

    const baseLabel = labelForBase(v.calculationMethod);
    const baseAmount = SAMPLE_BASIC;
    const computed = (baseAmount * pct) / 100;
    return { baseLabel, computed, baseAmount };
  });

  get isEdit(): boolean { return this.head !== null; }

  ngOnInit(): void {
    this.service.getAll({ pageSize: 200, isActive: true }).subscribe({
      next: (res) => {
        if (res.success && res.data) this.headOptions.set(res.data.items);
      },
    });

    this.form.valueChanges.subscribe(() => this.formValue.set(this.form.getRawValue()));
  }

  ngOnChanges(changes: SimpleChanges): void {
    if ('head' in changes) {
      if (this.head) {
        this.form.patchValue({
          headName: this.head.headName,
          headCode: this.head.headCode,
          headType: this.head.headType,
          calculationMethod: this.head.calculationMethod,
          percentage: this.head.percentage ?? null,
          baseHeadId: this.head.baseHeadId ?? null,
          isFixed: this.head.isFixed,
          isTaxable: this.head.isTaxable,
          isProvidentFundApplicable: this.head.isProvidentFundApplicable,
          displayOrder: this.head.displayOrder,
          description: this.head.description ?? '',
          isActive: this.head.isActive,
        });
      } else {
        this.form.reset({
          headName: '',
          headCode: '',
          headType: 'Earning',
          calculationMethod: 'Fixed',
          percentage: null,
          baseHeadId: null,
          isFixed: true,
          isTaxable: true,
          isProvidentFundApplicable: false,
          displayOrder: 10,
          description: '',
          isActive: true,
        });
      }
      this.formValue.set(this.form.getRawValue());
    }
  }

  submit(): void {
    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const isFixed = raw.calculationMethod === 'Fixed';
    const baseDto: CreateSalaryHeadDto = {
      headName: raw.headName.trim(),
      headCode: raw.headCode.trim().toUpperCase(),
      headType: raw.headType,
      calculationMethod: raw.calculationMethod,
      percentage: isFixed ? null : raw.percentage,
      baseHeadId: raw.calculationMethod === 'PercentageOfHead' ? raw.baseHeadId : null,
      isFixed: raw.isFixed,
      isTaxable: raw.isTaxable,
      isProvidentFundApplicable: raw.isProvidentFundApplicable,
      displayOrder: raw.displayOrder,
      description: raw.description?.trim() || null,
    };

    this.saving.set(true);
    const obs = this.isEdit && this.head
      ? this.service.update(this.head.id, { ...baseDto, isActive: raw.isActive } satisfies UpdateSalaryHeadDto)
      : this.service.create(baseDto);

    obs.subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success && res.data) {
          this.toast.success(this.isEdit ? 'Salary head updated.' : 'Salary head created.');
          this.saved.emit(res.data);
        } else {
          this.toast.error(res.message || 'Failed to save salary head.');
        }
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(err.error?.message || 'Failed to save salary head.');
      },
    });
  }

  hasError(field: keyof typeof this.form.controls, error: string): boolean {
    const ctrl = this.form.controls[field];
    return ctrl.touched && ctrl.hasError(error);
  }
}

function labelForBase(method: CalculationMethod): string {
  switch (method) {
    case 'PercentageOfBasic': return 'Basic Salary';
    case 'PercentageOfGross': return 'Gross Salary';
    case 'PercentageOfHead':  return 'Selected base head';
    case 'PercentageOfNet':   return 'Net Salary';
    default: return '';
  }
}

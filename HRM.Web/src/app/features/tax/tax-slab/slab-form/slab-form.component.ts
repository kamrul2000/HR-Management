import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import {
  FormArray,
  FormBuilder,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  heroArrowLeft,
  heroPlus,
  heroTrash,
  heroCheck,
} from '@ng-icons/heroicons/outline';

import { ToastService } from '../../../../core/services/toast.service';
import { LoadingSkeletonComponent } from '../../../../shared/components/loading-skeleton/loading-skeleton.component';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { CurrencyBdPipe } from '../../../../shared/pipes/currency-bd.pipe';
import {
  CreateTaxSlabConfigDto,
  TaxSlabDto,
  UpdateTaxSlabConfigDto,
} from '../../models/tax-slab.model';
import { TaxSlabService } from '../../services/tax-slab.service';

interface SlabFormGroup {
  slabOrder: FormControl<number>;
  minAmount: FormControl<number>;
  maxAmount: FormControl<number | null>;
  taxRate: FormControl<number>;
}

@Component({
  selector: 'hrm-tax-slab-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    NgIcon,
    PageHeaderComponent,
    LoadingSkeletonComponent,
    CurrencyBdPipe,
  ],
  providers: [provideIcons({ heroArrowLeft, heroPlus, heroTrash, heroCheck })],
  templateUrl: './slab-form.component.html',
  styleUrl: './slab-form.component.scss',
})
export class SlabFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(TaxSlabService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly editId = signal<number | null>(null);
  readonly loading = signal(false);
  readonly saving = signal(false);

  readonly form = this.fb.nonNullable.group({
    fiscalYear: ['', [Validators.required, Validators.maxLength(10)]],
    startDate: ['', [Validators.required]],
    endDate: ['', [Validators.required]],
    taxFreeThreshold: [0, [Validators.required, Validators.min(0)]],
    description: [''],
    isActive: [true],
    slabs: this.fb.array<FormGroup<SlabFormGroup>>([]),
  });

  get slabs(): FormArray<FormGroup<SlabFormGroup>> {
    return this.form.controls.slabs;
  }

  /** Aggregated peek so the user sees the structure hold together. */
  readonly summary = computed(() => {
    const slabs = this.slabs.controls.map((g) => g.getRawValue());
    const total = slabs.reduce((acc, s) => acc + (s.maxAmount ? s.maxAmount - s.minAmount : 0), 0);
    return { count: slabs.length, totalCovered: total };
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.editId.set(+id);
      this.load(+id);
    } else {
      this.addSlab();
    }
  }

  private load(id: number): void {
    this.loading.set(true);
    this.service.getById(id).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (!res.success || !res.data) {
          this.router.navigate(['/tax/slabs']);
          return;
        }
        const cfg = res.data;
        this.form.patchValue({
          fiscalYear: cfg.fiscalYear,
          startDate: cfg.startDate?.slice(0, 10),
          endDate: cfg.endDate?.slice(0, 10),
          taxFreeThreshold: cfg.taxFreeThreshold,
          description: cfg.description ?? '',
          isActive: cfg.isActive,
        });
        this.form.controls.fiscalYear.disable();
        this.form.controls.startDate.disable();
        this.form.controls.endDate.disable();
        this.slabs.clear();
        cfg.slabs
          .sort((a, b) => a.slabOrder - b.slabOrder)
          .forEach((s) => this.slabs.push(this.buildSlabGroup(s.slabOrder, s.minAmount, s.maxAmount ?? null, s.taxRate)));
      },
      error: () => {
        this.loading.set(false);
        this.router.navigate(['/tax/slabs']);
      },
    });
  }

  private buildSlabGroup(order: number, min: number, max: number | null, rate: number): FormGroup<SlabFormGroup> {
    return this.fb.nonNullable.group<SlabFormGroup>({
      slabOrder: this.fb.nonNullable.control(order, [Validators.required, Validators.min(1)]),
      minAmount: this.fb.nonNullable.control(min, [Validators.required, Validators.min(0)]),
      maxAmount: this.fb.control<number | null>(max),
      taxRate:   this.fb.nonNullable.control(rate, [Validators.required, Validators.min(0), Validators.max(100)]),
    });
  }

  addSlab(): void {
    const order = this.slabs.length + 1;
    const last = this.slabs.controls[this.slabs.length - 1]?.getRawValue();
    const min = last?.maxAmount ?? 0;
    this.slabs.push(this.buildSlabGroup(order, min, null, 0));
  }

  removeSlab(idx: number): void {
    this.slabs.removeAt(idx);
    this.slabs.controls.forEach((g, i) => g.patchValue({ slabOrder: i + 1 }));
  }

  save(): void {
    if (this.saving()) return;
    if (this.form.invalid || this.slabs.length === 0) {
      this.form.markAllAsTouched();
      this.toast.error('Fill required fields and at least one slab.');
      return;
    }
    const raw = this.form.getRawValue();
    const slabPayload: TaxSlabDto[] = raw.slabs.map((s, i) => ({
      slabOrder: i + 1,
      minAmount: +s.minAmount,
      maxAmount: s.maxAmount === null || s.maxAmount === undefined ? null : +s.maxAmount,
      taxRate: +s.taxRate,
    }));

    this.saving.set(true);

    if (this.editId()) {
      const dto: UpdateTaxSlabConfigDto = {
        taxFreeThreshold: +raw.taxFreeThreshold,
        description: raw.description?.trim() || null,
        isActive: raw.isActive,
        slabs: slabPayload,
      };
      this.service.update(this.editId()!, dto).subscribe({
        next: (res) => this.afterSave(res, 'updated'),
        error: (err: HttpErrorResponse) => this.afterError(err),
      });
    } else {
      const dto: CreateTaxSlabConfigDto = {
        fiscalYear: raw.fiscalYear.trim(),
        startDate: raw.startDate,
        endDate: raw.endDate,
        taxFreeThreshold: +raw.taxFreeThreshold,
        description: raw.description?.trim() || null,
        slabs: slabPayload,
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
      this.toast.success(`Tax configuration ${what}.`);
      this.router.navigate(['/tax/slabs']);
    } else {
      this.toast.error(res.message || `Failed to save.`);
    }
  }

  private afterError(err: HttpErrorResponse): void {
    this.saving.set(false);
    this.toast.error(err.error?.message || 'Failed to save.');
  }
}

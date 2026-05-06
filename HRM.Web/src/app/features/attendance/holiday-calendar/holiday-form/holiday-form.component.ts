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
  inject,
  signal,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { ToastService } from '../../../../core/services/toast.service';
import { DrawerComponent } from '../../../../shared/components/drawer/drawer.component';
import { BranchResponse } from '../../../organization/models/branch.model';
import { BranchService } from '../../../organization/services/branch.service';
import {
  CreateHolidayDto,
  HolidayResponse,
  HolidayType,
  HOLIDAY_TYPES,
  UpdateHolidayDto,
} from '../../models/holiday.model';
import { HolidayService } from '../../services/holiday.service';

type Scope = 'org' | 'branch';

@Component({
  selector: 'hrm-holiday-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DrawerComponent],
  templateUrl: './holiday-form.component.html',
})
export class HolidayFormComponent implements OnInit, OnChanges {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(HolidayService);
  private readonly branches = inject(BranchService);
  private readonly toast = inject(ToastService);

  @Input() holiday: HolidayResponse | null = null;
  @Input() defaultDate: string | null = null;

  @Output() saved = new EventEmitter<HolidayResponse>();
  @Output() dismiss = new EventEmitter<void>();

  readonly form = this.fb.nonNullable.group({
    holidayName: ['', [Validators.required, Validators.maxLength(150)]],
    holidayDate: ['', [Validators.required]],
    holidayType: ['Public' as HolidayType, [Validators.required]],
    description: ['', [Validators.maxLength(500)]],
    isRecurringYearly: [false],
    scope: ['org' as Scope],
    branchId: [null as number | null],
    isActive: [true],
  });

  readonly saving = signal(false);
  readonly branchOptions = signal<BranchResponse[]>([]);
  readonly types = HOLIDAY_TYPES;

  get isEdit(): boolean { return this.holiday !== null; }

  ngOnInit(): void {
    this.branches.getAll({ pageSize: 200, isActive: true }).subscribe({
      next: (res) => {
        if (res.success && res.data) this.branchOptions.set(res.data.items);
      },
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if ('holiday' in changes) {
      if (this.holiday) {
        this.form.patchValue({
          holidayName: this.holiday.holidayName,
          holidayDate: toDateInput(this.holiday.holidayDate),
          holidayType: this.holiday.holidayType,
          description: this.holiday.description ?? '',
          isRecurringYearly: this.holiday.isRecurringYearly,
          scope: this.holiday.branchId ? 'branch' : 'org',
          branchId: this.holiday.branchId ?? null,
          isActive: this.holiday.isActive,
        });
      } else {
        this.form.reset({
          holidayName: '',
          holidayDate: this.defaultDate ?? '',
          holidayType: 'Public',
          description: '',
          isRecurringYearly: false,
          scope: 'org',
          branchId: null,
          isActive: true,
        });
      }
    }
  }

  submit(): void {
    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    if (raw.scope === 'branch' && !raw.branchId) {
      this.toast.error('Please select a branch.');
      return;
    }

    const baseDto: CreateHolidayDto = {
      holidayName: raw.holidayName.trim(),
      holidayDate: raw.holidayDate,
      holidayType: raw.holidayType,
      description: raw.description?.trim() || null,
      isRecurringYearly: raw.isRecurringYearly,
      branchId: raw.scope === 'branch' ? raw.branchId : null,
    };

    this.saving.set(true);
    const obs = this.isEdit && this.holiday
      ? this.service.update(this.holiday.id, { ...baseDto, isActive: raw.isActive } satisfies UpdateHolidayDto)
      : this.service.create(baseDto);

    obs.subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success && res.data) {
          this.toast.success(this.isEdit ? 'Holiday updated.' : 'Holiday added.');
          this.saved.emit(res.data);
        } else {
          this.toast.error(res.message || 'Failed to save holiday.');
        }
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(err.error?.message || 'Failed to save holiday.');
      },
    });
  }

  hasError(field: keyof typeof this.form.controls, error: string): boolean {
    const ctrl = this.form.controls[field];
    return ctrl.touched && ctrl.hasError(error);
  }
}

function toDateInput(iso: string): string {
  return iso.length >= 10 ? iso.slice(0, 10) : iso;
}

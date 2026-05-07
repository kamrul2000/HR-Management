import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  EventEmitter,
  Input,
  OnChanges,
  Output,
  SimpleChanges,
  inject,
  signal,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { ToastService } from '../../../../core/services/toast.service';
import { DrawerComponent } from '../../../../shared/components/drawer/drawer.component';
import {
  CreateLeaveTypeDto,
  GenderRestriction,
  LeaveTypeResponse,
  UpdateLeaveTypeDto,
} from '../../models/leave-type.model';
import { LeaveTypeService } from '../../services/leave-type.service';

const GENDER_OPTIONS: { value: GenderRestriction; label: string }[] = [
  { value: 'All',    label: 'All employees' },
  { value: 'Male',   label: 'Male only' },
  { value: 'Female', label: 'Female only' },
];

@Component({
  selector: 'hrm-leave-type-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DrawerComponent],
  templateUrl: './leave-type-form.component.html',
  styles: [
    `
      .leave-type-form__toggles {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 8px 16px;
      }
    `,
  ],
})
export class LeaveTypeFormComponent implements OnChanges {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(LeaveTypeService);
  private readonly toast = inject(ToastService);

  @Input() leaveType: LeaveTypeResponse | null = null;

  @Output() saved = new EventEmitter<LeaveTypeResponse>();
  @Output() dismiss = new EventEmitter<void>();

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    code: ['', [Validators.required, Validators.maxLength(20)]],
    description: ['', [Validators.maxLength(500)]],
    isPaid: [true],
    isCarryForward: [false],
    maxCarryForwardDays: [0, [Validators.min(0), Validators.max(365)]],
    requiresApproval: [true],
    requiresDocument: [false],
    minNoticeDays: [0, [Validators.required, Validators.min(0), Validators.max(90)]],
    maxConsecutiveDays: [null as number | null],
    genderRestriction: ['All' as GenderRestriction],
    isActive: [true],
  });

  readonly saving = signal(false);
  readonly genders = GENDER_OPTIONS;

  get isEdit(): boolean { return this.leaveType !== null; }

  ngOnChanges(changes: SimpleChanges): void {
    if ('leaveType' in changes) {
      if (this.leaveType) {
        const lt = this.leaveType;
        this.form.patchValue({
          name: lt.name,
          code: lt.code,
          description: lt.description ?? '',
          isPaid: lt.isPaid,
          isCarryForward: lt.isCarryForward,
          maxCarryForwardDays: lt.maxCarryForwardDays ?? 0,
          requiresApproval: lt.requiresApproval,
          requiresDocument: lt.requiresDocument,
          minNoticeDays: lt.minNoticeDays ?? 0,
          maxConsecutiveDays: lt.maxConsecutiveDays ?? null,
          genderRestriction: (lt.genderRestriction as GenderRestriction) ?? 'All',
          isActive: lt.isActive,
        });
      } else {
        this.form.reset({
          name: '',
          code: '',
          description: '',
          isPaid: true,
          isCarryForward: false,
          maxCarryForwardDays: 0,
          requiresApproval: true,
          requiresDocument: false,
          minNoticeDays: 0,
          maxConsecutiveDays: null,
          genderRestriction: 'All',
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

    const baseDto: CreateLeaveTypeDto = {
      name: raw.name.trim(),
      code: raw.code.trim().toUpperCase(),
      description: raw.description?.trim() || null,
      isPaid: raw.isPaid,
      isCarryForward: raw.isCarryForward,
      maxCarryForwardDays: raw.isCarryForward ? raw.maxCarryForwardDays : 0,
      requiresApproval: raw.requiresApproval,
      requiresDocument: raw.requiresDocument,
      minNoticeDays: raw.minNoticeDays,
      maxConsecutiveDays: raw.maxConsecutiveDays && raw.maxConsecutiveDays > 0
        ? raw.maxConsecutiveDays
        : null,
      genderRestriction: raw.genderRestriction,
    };

    this.saving.set(true);
    const obs = this.isEdit && this.leaveType
      ? this.service.update(this.leaveType.id, { ...baseDto, isActive: raw.isActive } satisfies UpdateLeaveTypeDto)
      : this.service.create(baseDto);

    obs.subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success && res.data) {
          this.toast.success(this.isEdit ? 'Leave type updated.' : 'Leave type created.');
          this.saved.emit(res.data);
        } else {
          this.toast.error(res.message || 'Failed to save leave type.');
        }
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(err.error?.message || 'Failed to save leave type.');
      },
    });
  }

  hasError(field: keyof typeof this.form.controls, error: string): boolean {
    const ctrl = this.form.controls[field];
    return ctrl.touched && ctrl.hasError(error);
  }
}

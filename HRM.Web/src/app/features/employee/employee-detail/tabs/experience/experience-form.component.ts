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
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';

import { ToastService } from '../../../../../core/services/toast.service';
import { DrawerComponent } from '../../../../../shared/components/drawer/drawer.component';
import {
  CreateExperienceDto,
  ExperienceDto,
  UpdateExperienceDto,
} from '../../../models/additional-info.model';
import { AdditionalInfoService } from '../../../services/additional-info.service';

@Component({
  selector: 'hrm-experience-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DrawerComponent],
  templateUrl: './experience-form.component.html',
})
export class ExperienceFormComponent implements OnInit, OnChanges {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(AdditionalInfoService);
  private readonly toast = inject(ToastService);

  @Input({ required: true }) employeeId!: number;
  @Input() record: ExperienceDto | null = null;

  @Output() saved = new EventEmitter<ExperienceDto>();
  @Output() dismiss = new EventEmitter<void>();

  readonly form = this.fb.nonNullable.group(
    {
      organizationName: ['', [Validators.required, Validators.maxLength(200)]],
      designation: ['', [Validators.required, Validators.maxLength(150)]],
      fromDate: ['', [Validators.required]],
      toDate: [''],
      isCurrent: [false],
      responsibilities: ['', [Validators.maxLength(1000)]],
      reasonForLeaving: ['', [Validators.maxLength(500)]],
    },
    { validators: experienceCrossFieldValidator },
  );

  readonly saving = signal(false);

  get isEdit(): boolean { return this.record !== null; }

  ngOnInit(): void {
    // Toggling isCurrent should clear toDate.
    this.form.controls.isCurrent.valueChanges.subscribe((isCurrent) => {
      if (isCurrent) this.form.patchValue({ toDate: '' }, { emitEvent: false });
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if ('record' in changes) {
      if (this.record) {
        this.form.patchValue({
          organizationName: this.record.organizationName,
          designation: this.record.designation,
          fromDate: toDateInput(this.record.fromDate),
          toDate: this.record.toDate ? toDateInput(this.record.toDate) : '',
          isCurrent: this.record.isCurrent,
          responsibilities: this.record.responsibilities ?? '',
          reasonForLeaving: this.record.reasonForLeaving ?? '',
        });
      } else {
        this.form.reset({
          organizationName: '',
          designation: '',
          fromDate: '',
          toDate: '',
          isCurrent: false,
          responsibilities: '',
          reasonForLeaving: '',
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

    const baseDto: CreateExperienceDto = {
      organizationName: raw.organizationName.trim(),
      designation: raw.designation.trim(),
      fromDate: raw.fromDate,
      toDate: raw.isCurrent ? null : (raw.toDate || null),
      isCurrent: raw.isCurrent,
      responsibilities: raw.responsibilities?.trim() || null,
      reasonForLeaving: raw.isCurrent ? null : (raw.reasonForLeaving?.trim() || null),
    };

    this.saving.set(true);
    const obs = this.isEdit && this.record
      ? this.service.updateExperience(this.record.id, baseDto satisfies UpdateExperienceDto)
      : this.service.addExperience(this.employeeId, baseDto);

    obs.subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success && res.data) {
          this.toast.success(this.isEdit ? 'Experience updated.' : 'Experience added.');
          this.saved.emit(res.data);
        } else {
          this.toast.error(res.message || 'Failed to save experience.');
        }
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(err.error?.message || 'Failed to save experience.');
      },
    });
  }

  hasError(field: keyof typeof this.form.controls, error: string): boolean {
    const ctrl = this.form.controls[field];
    return ctrl.touched && ctrl.hasError(error);
  }

  formError(error: string): boolean {
    return this.form.touched && this.form.hasError(error);
  }
}

function experienceCrossFieldValidator(group: AbstractControl): ValidationErrors | null {
  const isCurrent = group.get('isCurrent')?.value as boolean;
  const fromDate = group.get('fromDate')?.value as string;
  const toDate = group.get('toDate')?.value as string;

  if (!isCurrent && !toDate) return { toRequired: true };
  if (!isCurrent && fromDate && toDate && toDate < fromDate) return { toBeforeFrom: true };
  return null;
}

function toDateInput(iso: string): string {
  return iso.length >= 10 ? iso.slice(0, 10) : iso;
}

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

import { ToastService } from '../../../../../core/services/toast.service';
import { DrawerComponent } from '../../../../../shared/components/drawer/drawer.component';
import {
  CreateEducationDto,
  EducationDto,
  UpdateEducationDto,
} from '../../../models/additional-info.model';
import { AdditionalInfoService } from '../../../services/additional-info.service';

@Component({
  selector: 'hrm-education-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DrawerComponent],
  templateUrl: './education-form.component.html',
})
export class EducationFormComponent implements OnChanges {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(AdditionalInfoService);
  private readonly toast = inject(ToastService);

  @Input({ required: true }) employeeId!: number;
  @Input() record: EducationDto | null = null;

  @Output() saved = new EventEmitter<EducationDto>();
  @Output() dismiss = new EventEmitter<void>();

  readonly form = this.fb.nonNullable.group({
    degree: ['', [Validators.required, Validators.maxLength(150)]],
    institution: ['', [Validators.required, Validators.maxLength(200)]],
    passingYear: [
      new Date().getFullYear(),
      [Validators.required, Validators.min(1950), Validators.max(2100)],
    ],
    majorSubject: ['', [Validators.maxLength(100)]],
    result: ['', [Validators.maxLength(50)]],
  });

  readonly saving = signal(false);

  get isEdit(): boolean { return this.record !== null; }

  ngOnChanges(changes: SimpleChanges): void {
    if ('record' in changes) {
      if (this.record) {
        this.form.patchValue({
          degree: this.record.degree,
          institution: this.record.institution,
          passingYear: this.record.passingYear,
          majorSubject: this.record.majorSubject ?? '',
          result: this.record.result ?? '',
        });
      } else {
        this.form.reset({
          degree: '',
          institution: '',
          passingYear: new Date().getFullYear(),
          majorSubject: '',
          result: '',
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
    const baseDto: CreateEducationDto = {
      degree: raw.degree.trim(),
      institution: raw.institution.trim(),
      passingYear: raw.passingYear,
      majorSubject: raw.majorSubject?.trim() || null,
      result: raw.result?.trim() || null,
    };

    this.saving.set(true);
    const obs = this.isEdit && this.record
      ? this.service.updateEducation(this.record.id, baseDto satisfies UpdateEducationDto)
      : this.service.addEducation(this.employeeId, baseDto);

    obs.subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success && res.data) {
          this.toast.success(this.isEdit ? 'Education updated.' : 'Education added.');
          this.saved.emit(res.data);
        } else {
          this.toast.error(res.message || 'Failed to save education.');
        }
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(err.error?.message || 'Failed to save education.');
      },
    });
  }

  hasError(field: keyof typeof this.form.controls, error: string): boolean {
    const ctrl = this.form.controls[field];
    return ctrl.touched && ctrl.hasError(error);
  }
}

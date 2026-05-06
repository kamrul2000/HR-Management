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
import { DepartmentResponse } from '../../models/department.model';
import {
  CreateDesignationDto,
  DesignationResponse,
  UpdateDesignationDto,
} from '../../models/designation.model';
import { DepartmentService } from '../../services/department.service';
import { DesignationService } from '../../services/designation.service';

@Component({
  selector: 'hrm-designation-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DrawerComponent],
  templateUrl: './designation-form.component.html',
})
export class DesignationFormComponent implements OnInit, OnChanges {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(DesignationService);
  private readonly departments = inject(DepartmentService);
  private readonly toast = inject(ToastService);

  @Input() designation: DesignationResponse | null = null;

  @Output() saved = new EventEmitter<DesignationResponse>();
  @Output() dismiss = new EventEmitter<void>();

  readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(150)]],
    grade: ['', [Validators.maxLength(50)]],
    description: ['', [Validators.maxLength(500)]],
    departmentId: [0, [Validators.required, Validators.min(1)]],
    isActive: [true],
  });

  readonly saving = signal(false);
  readonly departmentOptions = signal<DepartmentResponse[]>([]);
  readonly departmentsLoading = signal(true);

  get isEdit(): boolean { return this.designation !== null; }

  ngOnInit(): void {
    this.departments.getAll({ pageSize: 200, isActive: true }).subscribe({
      next: (res) => {
        this.departmentsLoading.set(false);
        if (res.success && res.data) this.departmentOptions.set(res.data.items);
      },
      error: () => this.departmentsLoading.set(false),
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if ('designation' in changes) {
      if (this.designation) {
        this.form.patchValue({
          title: this.designation.title,
          grade: this.designation.grade ?? '',
          description: this.designation.description ?? '',
          departmentId: this.designation.departmentId,
          isActive: this.designation.isActive,
        });
      } else {
        this.form.reset({
          title: '',
          grade: '',
          description: '',
          departmentId: 0,
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
    const grade = raw.grade?.trim() || null;
    const description = raw.description?.trim() || null;
    this.saving.set(true);

    const baseDto = {
      title: raw.title.trim(),
      grade,
      description,
      departmentId: raw.departmentId,
    };

    const obs = this.isEdit && this.designation
      ? this.service.update(this.designation.id, { ...baseDto, isActive: raw.isActive } satisfies UpdateDesignationDto)
      : this.service.create(baseDto satisfies CreateDesignationDto);

    obs.subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success && res.data) {
          this.toast.success(this.isEdit ? 'Designation updated.' : 'Designation created.');
          this.saved.emit(res.data);
        } else {
          this.toast.error(res.message || 'Failed to save designation.');
        }
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(err.error?.message || 'Failed to save designation.');
      },
    });
  }

  departmentLabel(d: DepartmentResponse): string {
    const parts = [d.name];
    if (d.branchName) parts.push(d.branchName);
    return parts.join(' • ');
  }

  hasError(field: keyof typeof this.form.controls, error: string): boolean {
    const ctrl = this.form.controls[field];
    return ctrl.touched && ctrl.hasError(error);
  }
}

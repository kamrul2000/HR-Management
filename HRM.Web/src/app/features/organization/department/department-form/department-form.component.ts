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
import { BranchResponse } from '../../models/branch.model';
import {
  CreateDepartmentDto,
  DepartmentResponse,
  UpdateDepartmentDto,
} from '../../models/department.model';
import { BranchService } from '../../services/branch.service';
import { DepartmentService } from '../../services/department.service';

@Component({
  selector: 'hrm-department-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DrawerComponent],
  templateUrl: './department-form.component.html',
})
export class DepartmentFormComponent implements OnInit, OnChanges {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(DepartmentService);
  private readonly branches = inject(BranchService);
  private readonly toast = inject(ToastService);

  @Input() department: DepartmentResponse | null = null;
  @Input() defaultBranchId: number | null = null;

  @Output() saved = new EventEmitter<DepartmentResponse>();
  @Output() dismiss = new EventEmitter<void>();

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(150)]],
    description: ['', [Validators.maxLength(500)]],
    branchId: [0, [Validators.required, Validators.min(1)]],
    isActive: [true],
  });

  readonly saving = signal(false);
  readonly branchOptions = signal<BranchResponse[]>([]);
  readonly branchesLoading = signal(true);

  get isEdit(): boolean { return this.department !== null; }

  ngOnInit(): void {
    this.branches.getAll({ pageSize: 200, isActive: true }).subscribe({
      next: (res) => {
        this.branchesLoading.set(false);
        if (res.success && res.data) this.branchOptions.set(res.data.items);
      },
      error: () => this.branchesLoading.set(false),
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if ('department' in changes) this.applyValue();
    if ('defaultBranchId' in changes && !this.department && this.defaultBranchId) {
      this.form.patchValue({ branchId: this.defaultBranchId });
    }
  }

  private applyValue(): void {
    if (this.department) {
      this.form.patchValue({
        name: this.department.name,
        description: this.department.description ?? '',
        branchId: this.department.branchId,
        isActive: this.department.isActive,
      });
    } else {
      this.form.reset({
        name: '',
        description: '',
        branchId: this.defaultBranchId ?? 0,
        isActive: true,
      });
    }
  }

  submit(): void {
    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const description = raw.description?.trim() || null;
    this.saving.set(true);

    const baseDto = {
      name: raw.name.trim(),
      description,
      branchId: raw.branchId,
    };

    const obs = this.isEdit && this.department
      ? this.service.update(this.department.id, { ...baseDto, isActive: raw.isActive } satisfies UpdateDepartmentDto)
      : this.service.create(baseDto satisfies CreateDepartmentDto);

    obs.subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success && res.data) {
          this.toast.success(this.isEdit ? 'Department updated.' : 'Department created.');
          this.saved.emit(res.data);
        } else {
          this.toast.error(res.message || 'Failed to save department.');
        }
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(err.error?.message || 'Failed to save department.');
      },
    });
  }

  branchLabel(b: BranchResponse): string {
    return b.companyName ? `${b.name} (${b.companyName})` : b.name;
  }

  hasError(field: keyof typeof this.form.controls, error: string): boolean {
    const ctrl = this.form.controls[field];
    return ctrl.touched && ctrl.hasError(error);
  }
}

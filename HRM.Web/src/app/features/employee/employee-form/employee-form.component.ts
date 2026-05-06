import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  heroArrowLeft,
  heroArrowRight,
  heroCheck,
  heroCheckCircle,
  heroCamera,
} from '@ng-icons/heroicons/outline';

import { ToastService } from '../../../core/services/toast.service';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { BranchResponse } from '../../organization/models/branch.model';
import { DepartmentResponse } from '../../organization/models/department.model';
import { DesignationResponse } from '../../organization/models/designation.model';
import { BranchService } from '../../organization/services/branch.service';
import { DepartmentService } from '../../organization/services/department.service';
import { DesignationService } from '../../organization/services/designation.service';
import {
  CreateEmployeeDto,
  EmployeeResponse,
  EmployeeStatus,
  EmploymentType,
  Gender,
  MaritalStatus,
  UpdateEmployeeDto,
} from '../models/employee.model';
import { PhotoUploadComponent } from '../photo-upload/photo-upload.component';
import { EmployeeService } from '../services/employee.service';

const GENDERS: Gender[] = ['Male', 'Female', 'Other'];
const MARITAL_STATUSES: MaritalStatus[] = ['Single', 'Married', 'Divorced', 'Widowed'];
const EMPLOYMENT_TYPES: EmploymentType[] = ['Permanent', 'Contract', 'Probationary', 'Internship'];
const STATUSES: EmployeeStatus[] = ['Active', 'Resigned', 'Terminated', 'Retired', 'Inactive'];

type WizardStep = 1 | 2 | 3 | 4; // 4 = success state

@Component({
  selector: 'hrm-employee-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    NgIcon,
    PageHeaderComponent,
    PhotoUploadComponent,
  ],
  providers: [
    provideIcons({
      heroArrowLeft,
      heroArrowRight,
      heroCheck,
      heroCheckCircle,
      heroCamera,
    }),
  ],
  templateUrl: './employee-form.component.html',
  styleUrl: './employee-form.component.scss',
})
export class EmployeeFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(EmployeeService);
  private readonly branches = inject(BranchService);
  private readonly departments = inject(DepartmentService);
  private readonly designations = inject(DesignationService);
  private readonly toast = inject(ToastService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly step = signal<WizardStep>(1);
  readonly saving = signal(false);
  readonly loading = signal(false);
  readonly editingId = signal<number | null>(null);
  readonly createdEmployee = signal<EmployeeResponse | null>(null);

  readonly branchOptions = signal<BranchResponse[]>([]);
  readonly departmentOptions = signal<DepartmentResponse[]>([]);
  readonly designationOptions = signal<DesignationResponse[]>([]);

  readonly genders = GENDERS;
  readonly maritalStatuses = MARITAL_STATUSES;
  readonly employmentTypes = EMPLOYMENT_TYPES;
  readonly statuses = STATUSES;

  readonly personalForm: FormGroup = this.fb.nonNullable.group({
    employeeCode: ['', [Validators.required, Validators.maxLength(30)]],
    firstName: ['', [Validators.required, Validators.maxLength(80)]],
    lastName: ['', [Validators.required, Validators.maxLength(80)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(150)]],
    phone: ['', [Validators.required, Validators.maxLength(20)]],
    dateOfBirth: ['', [Validators.required]],
    gender: ['Male' as Gender, [Validators.required]],
    maritalStatus: ['Single' as MaritalStatus, [Validators.required]],
    nationalId: ['', [Validators.maxLength(50)]],
    address: ['', [Validators.required, Validators.maxLength(500)]],
  });

  readonly employmentForm: FormGroup = this.fb.nonNullable.group({
    branchId: [0, [Validators.required, Validators.min(1)]],
    departmentId: [0, [Validators.required, Validators.min(1)]],
    designationId: [0, [Validators.required, Validators.min(1)]],
    employmentType: ['Permanent' as EmploymentType, [Validators.required]],
    joiningDate: ['', [Validators.required]],
    confirmationDate: [''],
    status: ['Active' as EmployeeStatus],
  });

  readonly isEdit = computed(() => this.editingId() !== null);
  readonly canAdvanceFrom1 = computed(() => this.personalForm.valid);
  readonly canAdvanceFrom2 = computed(() => this.employmentForm.valid);

  ngOnInit(): void {
    this.loadFilterOptions();

    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      const id = Number(idParam);
      this.editingId.set(id);
      this.loadEmployee(id);
    }
  }

  private loadFilterOptions(): void {
    this.branches.getAll({ pageSize: 200, isActive: true }).subscribe({
      next: (res) => {
        if (res.success && res.data) this.branchOptions.set(res.data.items);
      },
    });
    this.departments.getAll({ pageSize: 200, isActive: true }).subscribe({
      next: (res) => {
        if (res.success && res.data) this.departmentOptions.set(res.data.items);
      },
    });
    this.designations.getAll({ pageSize: 200, isActive: true }).subscribe({
      next: (res) => {
        if (res.success && res.data) this.designationOptions.set(res.data.items);
      },
    });
  }

  private loadEmployee(id: number): void {
    this.loading.set(true);
    this.service.getById(id).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success && res.data) this.applyEmployee(res.data);
      },
      error: () => this.loading.set(false),
    });
  }

  private applyEmployee(emp: EmployeeResponse): void {
    this.personalForm.patchValue({
      employeeCode: emp.employeeCode,
      firstName: emp.firstName,
      lastName: emp.lastName,
      email: emp.email,
      phone: emp.phone,
      dateOfBirth: this.toDateInput(emp.dateOfBirth),
      gender: emp.gender,
      maritalStatus: emp.maritalStatus,
      nationalId: emp.nationalId ?? '',
      address: emp.address,
    });
    this.employmentForm.patchValue({
      branchId: emp.branchId,
      departmentId: emp.departmentId,
      designationId: emp.designationId,
      employmentType: emp.employmentType,
      joiningDate: this.toDateInput(emp.joiningDate),
      confirmationDate: emp.confirmationDate ? this.toDateInput(emp.confirmationDate) : '',
      status: emp.status,
    });
    this.createdEmployee.set(emp);
  }

  // ─────────────────────────────────────────── derived options

  filteredDepartments = computed(() => {
    const branchId = this.employmentForm.get('branchId')?.value as number;
    if (!branchId) return this.departmentOptions();
    return this.departmentOptions().filter((d) => d.branchId === branchId);
  });

  filteredDesignations = computed(() => {
    const departmentId = this.employmentForm.get('departmentId')?.value as number;
    if (!departmentId) return this.designationOptions();
    return this.designationOptions().filter((d) => d.departmentId === departmentId);
  });

  // Re-trigger filtered computeds when dependent values change.
  // (Reactive forms emit synchronous valueChanges, but our `computed` doesn't
  // observe those — so we mirror values into local signals on change.)
  private readonly branchValue = signal<number>(0);
  private readonly departmentValue = signal<number>(0);

  onBranchChange(value: number): void {
    this.branchValue.set(value);
    // Reset cascading selections so they don't keep stale IDs.
    this.employmentForm.patchValue({ departmentId: 0, designationId: 0 });
  }

  onDepartmentChange(value: number): void {
    this.departmentValue.set(value);
    this.employmentForm.patchValue({ designationId: 0 });
  }

  // ─────────────────────────────────────────── nav

  next(): void {
    if (this.step() === 1) {
      if (!this.canAdvanceFrom1()) {
        this.personalForm.markAllAsTouched();
        return;
      }
      this.step.set(2);
    } else if (this.step() === 2) {
      if (!this.canAdvanceFrom2()) {
        this.employmentForm.markAllAsTouched();
        return;
      }
      this.step.set(3);
    }
  }

  back(): void {
    if (this.step() === 2) this.step.set(1);
    else if (this.step() === 3) this.step.set(2);
  }

  submit(): void {
    if (!this.canAdvanceFrom1() || !this.canAdvanceFrom2() || this.saving()) return;

    const personal = this.personalForm.getRawValue();
    const employment = this.employmentForm.getRawValue();

    const baseDto: CreateEmployeeDto = {
      employeeCode: personal.employeeCode.trim(),
      firstName: personal.firstName.trim(),
      lastName: personal.lastName.trim(),
      email: personal.email.trim(),
      phone: personal.phone.trim(),
      dateOfBirth: personal.dateOfBirth,
      gender: personal.gender,
      maritalStatus: personal.maritalStatus,
      nationalId: personal.nationalId?.trim() || null,
      address: personal.address.trim(),
      branchId: employment.branchId,
      departmentId: employment.departmentId,
      designationId: employment.designationId,
      employmentType: employment.employmentType,
      joiningDate: employment.joiningDate,
      confirmationDate: employment.confirmationDate || null,
    };

    this.saving.set(true);
    const id = this.editingId();
    const obs = id
      ? this.service.update(id, { ...baseDto, status: employment.status } satisfies UpdateEmployeeDto)
      : this.service.create(baseDto);

    obs.subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success && res.data) {
          this.toast.success(this.isEdit() ? 'Employee updated.' : 'Employee created.');
          this.createdEmployee.set(res.data);
          this.step.set(4);
        } else {
          this.toast.error(res.message || 'Failed to save employee.');
        }
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(err.error?.message || 'Failed to save employee.');
      },
    });
  }

  finish(): void {
    const created = this.createdEmployee();
    if (created) this.router.navigate(['/employees', created.id]);
    else this.router.navigate(['/employees']);
  }

  onPhotoUploaded(updated: EmployeeResponse): void {
    this.createdEmployee.set(updated);
  }

  hasPersonalError(field: string, error: string): boolean {
    const ctrl = this.personalForm.get(field);
    return !!ctrl && ctrl.touched && ctrl.hasError(error);
  }

  hasEmploymentError(field: string, error: string): boolean {
    const ctrl = this.employmentForm.get(field);
    return !!ctrl && ctrl.touched && ctrl.hasError(error);
  }

  // ─────────────────────────────────────────── lookup helpers used by review step

  branchName(id: number): string {
    return this.branchOptions().find((b) => b.id === id)?.name ?? '—';
  }

  departmentName(id: number): string {
    return this.departmentOptions().find((d) => d.id === id)?.name ?? '—';
  }

  designationTitle(id: number): string {
    return this.designationOptions().find((d) => d.id === id)?.title ?? '—';
  }

  private toDateInput(iso: string): string {
    if (!iso) return '';
    return iso.length >= 10 ? iso.slice(0, 10) : iso;
  }
}

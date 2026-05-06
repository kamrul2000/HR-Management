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
import {
  BranchResponse,
  CreateBranchDto,
  UpdateBranchDto,
} from '../../models/branch.model';
import { CompanyResponse } from '../../models/company.model';
import { BranchService } from '../../services/branch.service';
import { CompanyService } from '../../services/company.service';

@Component({
  selector: 'hrm-branch-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DrawerComponent],
  templateUrl: './branch-form.component.html',
})
export class BranchFormComponent implements OnInit, OnChanges {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(BranchService);
  private readonly companies = inject(CompanyService);
  private readonly toast = inject(ToastService);

  @Input() branch: BranchResponse | null = null;
  /** Pre-select a company when adding from a company-filtered view. */
  @Input() defaultCompanyId: number | null = null;

  @Output() saved = new EventEmitter<BranchResponse>();
  @Output() dismiss = new EventEmitter<void>();

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(150)]],
    code: ['', [Validators.required, Validators.maxLength(20)]],
    address: ['', [Validators.required, Validators.maxLength(500)]],
    phone: ['', [Validators.required, Validators.maxLength(20)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(150)]],
    managerName: ['', [Validators.maxLength(100)]],
    companyId: [0, [Validators.required, Validators.min(1)]],
    isActive: [true],
  });

  readonly saving = signal(false);
  readonly companyOptions = signal<CompanyResponse[]>([]);
  readonly companiesLoading = signal(true);

  get isEdit(): boolean { return this.branch !== null; }

  ngOnInit(): void {
    this.companies.getAll({ pageSize: 200, isActive: true }).subscribe({
      next: (res) => {
        this.companiesLoading.set(false);
        if (res.success && res.data) this.companyOptions.set(res.data.items);
      },
      error: () => this.companiesLoading.set(false),
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if ('branch' in changes) this.applyBranch();
    if ('defaultCompanyId' in changes && !this.branch && this.defaultCompanyId) {
      this.form.patchValue({ companyId: this.defaultCompanyId });
    }
  }

  private applyBranch(): void {
    if (this.branch) {
      this.form.patchValue({
        name: this.branch.name,
        code: this.branch.code,
        address: this.branch.address,
        phone: this.branch.phone,
        email: this.branch.email,
        managerName: this.branch.managerName ?? '',
        companyId: this.branch.companyId,
        isActive: this.branch.isActive,
      });
    } else {
      this.form.reset({
        name: '',
        code: '',
        address: '',
        phone: '',
        email: '',
        managerName: '',
        companyId: this.defaultCompanyId ?? 0,
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
    const trimmedManager = raw.managerName?.trim() || null;
    this.saving.set(true);

    const baseDto = {
      name: raw.name.trim(),
      code: raw.code.trim().toUpperCase(),
      address: raw.address.trim(),
      phone: raw.phone.trim(),
      email: raw.email.trim(),
      managerName: trimmedManager,
      companyId: raw.companyId,
    };

    const obs = this.isEdit && this.branch
      ? this.service.update(this.branch.id, { ...baseDto, isActive: raw.isActive } satisfies UpdateBranchDto)
      : this.service.create(baseDto satisfies CreateBranchDto);

    obs.subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success && res.data) {
          this.toast.success(this.isEdit ? 'Branch updated.' : 'Branch created.');
          this.saved.emit(res.data);
        } else {
          this.toast.error(res.message || 'Failed to save branch.');
        }
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(err.error?.message || 'Failed to save branch.');
      },
    });
  }

  hasError(field: keyof typeof this.form.controls, error: string): boolean {
    const ctrl = this.form.controls[field];
    return ctrl.touched && ctrl.hasError(error);
  }
}

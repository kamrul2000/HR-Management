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
import { FileUploadComponent } from '../../../../shared/components/file-upload/file-upload.component';
import {
  CompanyResponse,
  CreateCompanyDto,
  UpdateCompanyDto,
} from '../../models/company.model';
import { CompanyService } from '../../services/company.service';

@Component({
  selector: 'hrm-company-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DrawerComponent, FileUploadComponent],
  templateUrl: './company-form.component.html',
  styleUrl: './company-form.component.scss',
})
export class CompanyFormComponent implements OnChanges {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(CompanyService);
  private readonly toast = inject(ToastService);

  /** Pass an existing company to edit. Omit (or pass null) to create. */
  @Input() company: CompanyResponse | null = null;

  @Output() saved = new EventEmitter<CompanyResponse>();
  @Output() dismiss = new EventEmitter<void>();

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(150)]],
    address: ['', [Validators.required, Validators.maxLength(500)]],
    phone: ['', [Validators.required, Validators.maxLength(20)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(150)]],
    website: ['', [Validators.maxLength(200)]],
    isActive: [true],
  });

  readonly saving = signal(false);
  readonly uploadingLogo = signal(false);

  get isEdit(): boolean {
    return this.company !== null;
  }

  ngOnChanges(changes: SimpleChanges): void {
    if ('company' in changes) {
      if (this.company) {
        this.form.patchValue({
          name: this.company.name,
          address: this.company.address,
          phone: this.company.phone,
          email: this.company.email,
          website: this.company.website ?? '',
          isActive: this.company.isActive,
        });
      } else {
        this.form.reset({
          name: '',
          address: '',
          phone: '',
          email: '',
          website: '',
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
    const trimmedWebsite = raw.website?.trim() || null;

    this.saving.set(true);

    if (this.isEdit && this.company) {
      const dto: UpdateCompanyDto = {
        name: raw.name.trim(),
        address: raw.address.trim(),
        phone: raw.phone.trim(),
        email: raw.email.trim(),
        website: trimmedWebsite,
        isActive: raw.isActive,
      };
      this.service.update(this.company.id, dto).subscribe({
        next: (res) => {
          this.saving.set(false);
          if (res.success && res.data) {
            this.toast.success('Company updated.');
            this.saved.emit(res.data);
          } else {
            this.toast.error(res.message || 'Failed to update company.');
          }
        },
        error: (err: HttpErrorResponse) => {
          this.saving.set(false);
          this.toast.error(err.error?.message || 'Failed to update company.');
        },
      });
    } else {
      const dto: CreateCompanyDto = {
        name: raw.name.trim(),
        address: raw.address.trim(),
        phone: raw.phone.trim(),
        email: raw.email.trim(),
        website: trimmedWebsite,
      };
      this.service.create(dto).subscribe({
        next: (res) => {
          this.saving.set(false);
          if (res.success && res.data) {
            this.toast.success('Company created.');
            this.saved.emit(res.data);
          } else {
            this.toast.error(res.message || 'Failed to create company.');
          }
        },
        error: (err: HttpErrorResponse) => {
          this.saving.set(false);
          this.toast.error(err.error?.message || 'Failed to create company.');
        },
      });
    }
  }

  onLogoSelected(file: File): void {
    if (!this.company) return;
    this.uploadingLogo.set(true);
    this.service.uploadLogo(this.company.id, file).subscribe({
      next: (res) => {
        this.uploadingLogo.set(false);
        if (res.success && res.data) {
          this.company = res.data;
          this.toast.success('Logo uploaded.');
          this.saved.emit(res.data);
        } else {
          this.toast.error(res.message || 'Failed to upload logo.');
        }
      },
      error: (err: HttpErrorResponse) => {
        this.uploadingLogo.set(false);
        this.toast.error(err.error?.message || 'Failed to upload logo.');
      },
    });
  }

  onUploadError(message: string): void {
    this.toast.error(message);
  }

  hasError(field: keyof typeof this.form.controls, error: string): boolean {
    const ctrl = this.form.controls[field];
    return ctrl.touched && ctrl.hasError(error);
  }
}

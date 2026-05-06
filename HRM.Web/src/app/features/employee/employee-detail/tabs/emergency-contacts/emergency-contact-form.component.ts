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
  CreateEmergencyContactDto,
  EmergencyContactDto,
  UpdateEmergencyContactDto,
} from '../../../models/additional-info.model';
import { AdditionalInfoService } from '../../../services/additional-info.service';

@Component({
  selector: 'hrm-emergency-contact-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DrawerComponent],
  templateUrl: './emergency-contact-form.component.html',
})
export class EmergencyContactFormComponent implements OnChanges {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(AdditionalInfoService);
  private readonly toast = inject(ToastService);

  @Input({ required: true }) employeeId!: number;
  @Input() contact: EmergencyContactDto | null = null;

  @Output() saved = new EventEmitter<EmergencyContactDto>();
  @Output() dismiss = new EventEmitter<void>();

  readonly form = this.fb.nonNullable.group({
    contactName: ['', [Validators.required, Validators.maxLength(150)]],
    relationship: ['', [Validators.required, Validators.maxLength(50)]],
    phone: ['', [Validators.required, Validators.maxLength(20)]],
    alternatePhone: ['', [Validators.maxLength(20)]],
    address: ['', [Validators.maxLength(500)]],
    isPrimary: [false],
  });

  readonly saving = signal(false);

  get isEdit(): boolean { return this.contact !== null; }

  ngOnChanges(changes: SimpleChanges): void {
    if ('contact' in changes) {
      if (this.contact) {
        this.form.patchValue({
          contactName: this.contact.contactName,
          relationship: this.contact.relationship,
          phone: this.contact.phone,
          alternatePhone: this.contact.alternatePhone ?? '',
          address: this.contact.address ?? '',
          isPrimary: this.contact.isPrimary,
        });
      } else {
        this.form.reset({
          contactName: '',
          relationship: '',
          phone: '',
          alternatePhone: '',
          address: '',
          isPrimary: false,
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
    const baseDto: CreateEmergencyContactDto = {
      contactName: raw.contactName.trim(),
      relationship: raw.relationship.trim(),
      phone: raw.phone.trim(),
      alternatePhone: raw.alternatePhone?.trim() || null,
      address: raw.address?.trim() || null,
      isPrimary: raw.isPrimary,
    };

    this.saving.set(true);
    const obs = this.isEdit && this.contact
      ? this.service.updateContact(this.contact.id, baseDto satisfies UpdateEmergencyContactDto)
      : this.service.addContact(this.employeeId, baseDto);

    obs.subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success && res.data) {
          this.toast.success(this.isEdit ? 'Contact updated.' : 'Contact added.');
          this.saved.emit(res.data);
        } else {
          this.toast.error(res.message || 'Failed to save contact.');
        }
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(err.error?.message || 'Failed to save contact.');
      },
    });
  }

  hasError(field: keyof typeof this.form.controls, error: string): boolean {
    const ctrl = this.form.controls[field];
    return ctrl.touched && ctrl.hasError(error);
  }
}

import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  EventEmitter,
  Input,
  OnChanges,
  Output,
  SimpleChanges,
  computed,
  inject,
  signal,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { ToastService } from '../../../../core/services/toast.service';
import { DrawerComponent } from '../../../../shared/components/drawer/drawer.component';
import {
  CreateDutySlotDto,
  DutySlotResponse,
  UpdateDutySlotDto,
} from '../../models/duty-slot.model';
import { DutySlotService } from '../../services/duty-slot.service';

@Component({
  selector: 'hrm-duty-slot-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DrawerComponent],
  templateUrl: './duty-slot-form.component.html',
  styleUrl: './duty-slot-form.component.scss',
})
export class DutySlotFormComponent implements OnChanges {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(DutySlotService);
  private readonly toast = inject(ToastService);

  @Input() slot: DutySlotResponse | null = null;

  @Output() saved = new EventEmitter<DutySlotResponse>();
  @Output() dismiss = new EventEmitter<void>();

  readonly form = this.fb.nonNullable.group({
    slotName: ['', [Validators.required, Validators.maxLength(100)]],
    startTime: ['09:00', [Validators.required]],
    endTime: ['18:00', [Validators.required]],
    breakDurationMinutes: [60, [Validators.required, Validators.min(0), Validators.max(480)]],
    lateToleranceMinutes: [10, [Validators.required, Validators.min(0), Validators.max(120)]],
    isActive: [true],
  });

  readonly saving = signal(false);

  /** Re-render the live preview every time the form value changes. */
  readonly formValue = signal(this.form.getRawValue());

  readonly preview = computed(() => {
    const { startTime, endTime, breakDurationMinutes } = this.formValue();
    if (!startTime || !endTime) {
      return { totalMinutes: 0, label: '—', isNightShift: false };
    }
    const start = parseTime(startTime);
    const end = parseTime(endTime);
    if (start === null || end === null) {
      return { totalMinutes: 0, label: '—', isNightShift: false };
    }

    const isNightShift = end <= start;
    const rawMinutes = isNightShift ? (24 * 60 - start) + end : end - start;
    const totalMinutes = Math.max(0, rawMinutes - (breakDurationMinutes ?? 0));

    const hours = Math.floor(totalMinutes / 60);
    const minutes = totalMinutes % 60;
    const label = minutes > 0 ? `${hours}h ${minutes}m` : `${hours}h`;
    return { totalMinutes, label, isNightShift };
  });

  get isEdit(): boolean { return this.slot !== null; }

  constructor() {
    this.form.valueChanges.subscribe((v) => {
      this.formValue.set({ ...this.form.getRawValue(), ...v });
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if ('slot' in changes) {
      if (this.slot) {
        this.form.patchValue({
          slotName: this.slot.slotName,
          startTime: trimTime(this.slot.startTime),
          endTime: trimTime(this.slot.endTime),
          breakDurationMinutes: this.slot.breakDurationMinutes,
          lateToleranceMinutes: this.slot.lateToleranceMinutes,
          isActive: this.slot.isActive,
        });
      } else {
        this.form.reset({
          slotName: '',
          startTime: '09:00',
          endTime: '18:00',
          breakDurationMinutes: 60,
          lateToleranceMinutes: 10,
          isActive: true,
        });
      }
      this.formValue.set(this.form.getRawValue());
    }
  }

  submit(): void {
    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const baseDto: CreateDutySlotDto = {
      slotName: raw.slotName.trim(),
      startTime: toTimeSpan(raw.startTime),
      endTime: toTimeSpan(raw.endTime),
      breakDurationMinutes: raw.breakDurationMinutes,
      lateToleranceMinutes: raw.lateToleranceMinutes,
    };

    this.saving.set(true);
    const obs = this.isEdit && this.slot
      ? this.service.update(this.slot.id, { ...baseDto, isActive: raw.isActive } satisfies UpdateDutySlotDto)
      : this.service.create(baseDto);

    obs.subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success && res.data) {
          this.toast.success(this.isEdit ? 'Duty slot updated.' : 'Duty slot created.');
          this.saved.emit(res.data);
        } else {
          this.toast.error(res.message || 'Failed to save duty slot.');
        }
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(err.error?.message || 'Failed to save duty slot.');
      },
    });
  }

  hasError(field: keyof typeof this.form.controls, error: string): boolean {
    const ctrl = this.form.controls[field];
    return ctrl.touched && ctrl.hasError(error);
  }
}

function parseTime(value: string): number | null {
  if (!value || !value.includes(':')) return null;
  const [h, m] = value.split(':').map(Number);
  if (Number.isNaN(h) || Number.isNaN(m)) return null;
  return h * 60 + m;
}

function trimTime(value: string): string {
  return value.length >= 5 ? value.slice(0, 5) : value;
}

function toTimeSpan(value: string): string {
  // HTML <input type="time"> returns "HH:mm". Backend TimeSpan needs "HH:mm:ss".
  return value.length === 5 ? `${value}:00` : value;
}

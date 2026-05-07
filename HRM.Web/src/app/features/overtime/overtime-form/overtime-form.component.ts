import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { heroArrowLeft, heroCheck, heroExclamationTriangle } from '@ng-icons/heroicons/outline';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';

import { ToastService } from '../../../core/services/toast.service';
import { AvatarComponent } from '../../../shared/components/avatar/avatar.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { AttendanceResponse } from '../../attendance/models/attendance.model';
import { AttendanceService } from '../../attendance/services/attendance.service';
import { EmployeeResponse } from '../../employee/models/employee.model';
import { EmployeeService } from '../../employee/services/employee.service';
import { CreateOvertimeDto, OvertimeType } from '../models/overtime.model';
import { OvertimeService } from '../services/overtime.service';

const TYPE_OPTIONS: OvertimeType[] = ['Regular', 'Holiday', 'WeeklyOff'];

@Component({
  selector: 'hrm-overtime-form',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterLink,
    NgIcon,
    PageHeaderComponent,
    AvatarComponent,
  ],
  providers: [provideIcons({ heroArrowLeft, heroCheck, heroExclamationTriangle })],
  templateUrl: './overtime-form.component.html',
  styleUrl: './overtime-form.component.scss',
})
export class OvertimeFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(OvertimeService);
  private readonly attendance = inject(AttendanceService);
  private readonly employees = inject(EmployeeService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);

  readonly form = this.fb.nonNullable.group({
    employeeId: [0, [Validators.required, Validators.min(1)]],
    employeeLabel: [''],
    overtimeDate: ['', [Validators.required]],
    overtimeType: ['Regular' as OvertimeType, [Validators.required]],
    requestedMinutes: [0, [Validators.required, Validators.min(1), Validators.max(720)]],
    reason: ['', [Validators.required, Validators.maxLength(500)]],
  });

  readonly saving = signal(false);
  readonly searchResults = signal<EmployeeResponse[]>([]);
  readonly attendanceRecord = signal<AttendanceResponse | null>(null);
  readonly attendanceLoading = signal(false);
  readonly attendanceError = signal<string | null>(null);

  readonly typeOptions = TYPE_OPTIONS;

  private readonly search$ = new Subject<string>();

  readonly canSubmit = computed(() => this.form.valid && !!this.attendanceRecord() && !this.saving());

  ngOnInit(): void {
    this.search$.pipe(debounceTime(300), distinctUntilChanged()).subscribe((term) => {
      this.runEmployeeSearch(term);
    });

    // Refresh attendance whenever employee or date changes.
    this.form.controls.employeeId.valueChanges.subscribe(() => this.refreshAttendance());
    this.form.controls.overtimeDate.valueChanges.subscribe(() => this.refreshAttendance());
  }

  onEmployeeSearch(term: string): void {
    this.form.patchValue({ employeeLabel: term, employeeId: 0 });
    this.attendanceRecord.set(null);
    this.search$.next(term.trim());
  }

  private runEmployeeSearch(term: string): void {
    if (!term) {
      this.searchResults.set([]);
      return;
    }
    this.employees.getAll({ search: term, status: 'Active', pageSize: 8 }).subscribe({
      next: (res) => {
        if (res.success && res.data) this.searchResults.set(res.data.items);
      },
    });
  }

  pickEmployee(emp: EmployeeResponse): void {
    this.form.patchValue({
      employeeId: emp.id,
      employeeLabel: `${emp.fullName} (${emp.employeeCode})`,
    });
    this.searchResults.set([]);
  }

  private refreshAttendance(): void {
    const employeeId = this.form.value.employeeId ?? 0;
    const date = this.form.value.overtimeDate ?? '';
    if (!employeeId || !date) {
      this.attendanceRecord.set(null);
      this.attendanceError.set(null);
      return;
    }
    this.attendanceLoading.set(true);
    this.attendanceError.set(null);
    this.attendance
      .getAll({ employeeId, fromDate: date, toDate: date, pageSize: 1 })
      .subscribe({
        next: (res) => {
          this.attendanceLoading.set(false);
          const record = res.success && res.data && res.data.items.length > 0
            ? res.data.items[0]
            : null;
          this.attendanceRecord.set(record);
          if (!record) {
            this.attendanceError.set(
              'No attendance recorded for this employee on this date.',
            );
          } else if (record.overtimeMinutes > 0) {
            // Pre-fill requested minutes with what attendance captured.
            this.form.patchValue({ requestedMinutes: record.overtimeMinutes });
          }
        },
        error: () => {
          this.attendanceLoading.set(false);
          this.attendanceError.set('Could not load attendance for this date.');
        },
      });
  }

  submit(): void {
    if (!this.canSubmit() || !this.attendanceRecord()) {
      this.form.markAllAsTouched();
      return;
    }
    const raw = this.form.getRawValue();
    const att = this.attendanceRecord()!;

    const dto: CreateOvertimeDto = {
      employeeId: raw.employeeId,
      attendanceId: att.id,
      overtimeDate: raw.overtimeDate,
      overtimeType: raw.overtimeType,
      requestedMinutes: raw.requestedMinutes,
      reason: raw.reason.trim(),
    };

    this.saving.set(true);
    this.service.create(dto).subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success) {
          this.toast.success('Overtime request submitted.');
          this.router.navigate(['/overtime']);
        } else {
          this.toast.error(res.message || 'Failed to submit overtime.');
        }
      },
      error: (err: HttpErrorResponse) => {
        this.saving.set(false);
        this.toast.error(err.error?.message || 'Failed to submit overtime.');
      },
    });
  }

  hasError(field: keyof typeof this.form.controls, error: string): boolean {
    const ctrl = this.form.controls[field];
    return ctrl.touched && ctrl.hasError(error);
  }

  formatMinutes(minutes: number): string {
    if (!minutes) return '0m';
    const h = Math.floor(minutes / 60);
    const m = minutes % 60;
    if (h && m) return `${h}h ${m}m`;
    if (h) return `${h}h`;
    return `${m}m`;
  }
}

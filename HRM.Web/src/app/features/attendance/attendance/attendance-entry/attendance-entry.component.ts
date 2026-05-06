import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  heroArrowLeft,
  heroCheck,
  heroPlay,
} from '@ng-icons/heroicons/outline';

import { ToastService } from '../../../../core/services/toast.service';
import { AvatarComponent } from '../../../../shared/components/avatar/avatar.component';
import { LoadingSkeletonComponent } from '../../../../shared/components/loading-skeleton/loading-skeleton.component';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { EmployeeResponse } from '../../../employee/models/employee.model';
import { EmployeeService } from '../../../employee/services/employee.service';
import { BranchResponse } from '../../../organization/models/branch.model';
import { BranchService } from '../../../organization/services/branch.service';
import {
  AttendanceStatus,
  BulkAttendanceRow,
} from '../../models/attendance.model';
import { DutySlotResponse } from '../../models/duty-slot.model';
import { AttendanceService } from '../../services/attendance.service';
import { DutySlotService } from '../../services/duty-slot.service';

interface EditableRow extends BulkAttendanceRow {
  employee: EmployeeResponse;
}

const STATUSES: AttendanceStatus[] = [
  'Present',
  'Absent',
  'Late',
  'HalfDay',
  'Holiday',
  'WeeklyOff',
  'OnLeave',
];

@Component({
  selector: 'hrm-attendance-entry',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    NgIcon,
    PageHeaderComponent,
    LoadingSkeletonComponent,
    AvatarComponent,
  ],
  providers: [provideIcons({ heroArrowLeft, heroCheck, heroPlay })],
  templateUrl: './attendance-entry.component.html',
  styleUrl: './attendance-entry.component.scss',
})
export class AttendanceEntryComponent implements OnInit {
  private readonly attendance = inject(AttendanceService);
  private readonly dutySlots = inject(DutySlotService);
  private readonly branches = inject(BranchService);
  private readonly employees = inject(EmployeeService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);

  readonly statuses = STATUSES;
  readonly attendanceDate = signal(today());
  readonly dutySlotId = signal<number | null>(null);
  readonly branchId = signal<number | null>(null);

  readonly slotOptions = signal<DutySlotResponse[]>([]);
  readonly branchOptions = signal<BranchResponse[]>([]);

  readonly rows = signal<EditableRow[]>([]);
  readonly loadingEmployees = signal(false);
  readonly submitting = signal(false);

  readonly canLoad = computed(() => !!this.dutySlotId() && !!this.attendanceDate());
  readonly canSubmit = computed(() => this.rows().length > 0 && !this.submitting());

  ngOnInit(): void {
    this.dutySlots.getAll({ isActive: true, pageSize: 100 }).subscribe({
      next: (res) => {
        if (res.success && res.data) this.slotOptions.set(res.data.items);
      },
    });
    this.branches.getAll({ pageSize: 200, isActive: true }).subscribe({
      next: (res) => {
        if (res.success && res.data) this.branchOptions.set(res.data.items);
      },
    });
  }

  loadEmployees(): void {
    if (!this.canLoad()) return;
    this.loadingEmployees.set(true);
    this.employees
      .getAll({
        branchId: this.branchId() ?? undefined,
        status: 'Active',
        pageSize: 200,
      })
      .subscribe({
        next: (res) => {
          this.loadingEmployees.set(false);
          if (res.success && res.data) {
            const rows = res.data.items.map<EditableRow>((emp) => ({
              employee: emp,
              employeeId: emp.id,
              status: 'Present',
              punchInTime: defaultPunch(this.attendanceDate(), '09:00'),
              punchOutTime: defaultPunch(this.attendanceDate(), '18:00'),
              remarks: null,
            }));
            this.rows.set(rows);
          }
        },
        error: () => this.loadingEmployees.set(false),
      });
  }

  setRowStatus(employeeId: number, status: AttendanceStatus): void {
    this.rows.update((rows) =>
      rows.map((r) => {
        if (r.employeeId !== employeeId) return r;
        // Clear punches when the status implies no on-site presence.
        const cleared = ['Absent', 'Holiday', 'WeeklyOff', 'OnLeave'].includes(status);
        return {
          ...r,
          status,
          punchInTime: cleared ? null : r.punchInTime,
          punchOutTime: cleared ? null : r.punchOutTime,
        };
      }),
    );
  }

  setRowField<K extends keyof BulkAttendanceRow>(
    employeeId: number,
    field: K,
    value: BulkAttendanceRow[K],
  ): void {
    this.rows.update((rows) =>
      rows.map((r) => (r.employeeId === employeeId ? { ...r, [field]: value } : r)),
    );
  }

  removeRow(employeeId: number): void {
    this.rows.update((rows) => rows.filter((r) => r.employeeId !== employeeId));
  }

  applyDefaults(): void {
    this.rows.update((rows) =>
      rows.map((r) => ({
        ...r,
        status: 'Present',
        punchInTime: defaultPunch(this.attendanceDate(), '09:00'),
        punchOutTime: defaultPunch(this.attendanceDate(), '18:00'),
      })),
    );
  }

  submit(): void {
    if (!this.canSubmit() || !this.dutySlotId()) return;

    this.submitting.set(true);
    this.attendance
      .bulkCreate({
        attendanceDate: this.attendanceDate(),
        dutySlotId: this.dutySlotId()!,
        rows: this.rows().map((r) => ({
          employeeId: r.employeeId,
          status: r.status,
          punchInTime: r.punchInTime ? toIsoDateTime(this.attendanceDate(), r.punchInTime) : null,
          punchOutTime: r.punchOutTime ? toIsoDateTime(this.attendanceDate(), r.punchOutTime) : null,
          remarks: r.remarks?.trim() || null,
        })),
      })
      .subscribe({
        next: (res) => {
          this.submitting.set(false);
          if (res.success) {
            this.toast.success('Attendance saved.');
            this.router.navigate(['/attendance/records']);
          } else {
            this.toast.error(res.message || 'Failed to save attendance.');
          }
        },
        error: (err: HttpErrorResponse) => {
          this.submitting.set(false);
          this.toast.error(err.error?.message || 'Failed to save attendance.');
        },
      });
  }
}

function today(): string {
  return new Date().toISOString().slice(0, 10);
}

function defaultPunch(date: string, time: string): string {
  return toIsoDateTime(date, time);
}

function toIsoDateTime(date: string, time: string): string {
  // Build a local-time ISO-ish string the backend will parse as DateTime.
  // Format: YYYY-MM-DDTHH:mm:00
  const t = time.length === 5 ? `${time}:00` : time;
  return `${date}T${t}`;
}

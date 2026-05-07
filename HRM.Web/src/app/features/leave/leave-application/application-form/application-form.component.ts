import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { heroArrowLeft, heroCheck, heroExclamationTriangle } from '@ng-icons/heroicons/outline';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';

import { ToastService } from '../../../../core/services/toast.service';
import { AvatarComponent } from '../../../../shared/components/avatar/avatar.component';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header.component';
import { EmployeeResponse } from '../../../employee/models/employee.model';
import { EmployeeService } from '../../../employee/services/employee.service';
import { LeaveBalanceDto } from '../../models/leave-allotment.model';
import { LeaveTypeResponse } from '../../models/leave-type.model';
import { LeaveAllotmentService } from '../../services/leave-allotment.service';
import { LeaveApplicationService } from '../../services/leave-application.service';
import { LeaveTypeService } from '../../services/leave-type.service';

@Component({
  selector: 'hrm-application-form',
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
  templateUrl: './application-form.component.html',
  styleUrl: './application-form.component.scss',
})
export class ApplicationFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly applications = inject(LeaveApplicationService);
  private readonly leaveTypes = inject(LeaveTypeService);
  private readonly allotments = inject(LeaveAllotmentService);
  private readonly employees = inject(EmployeeService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);

  readonly form = this.fb.nonNullable.group({
    employeeId: [0, [Validators.required, Validators.min(1)]],
    employeeLabel: [''],
    leaveTypeId: [0, [Validators.required, Validators.min(1)]],
    fromDate: ['', [Validators.required]],
    toDate: ['', [Validators.required]],
    reason: ['', [Validators.required, Validators.maxLength(1000)]],
  });

  readonly saving = signal(false);
  readonly leaveTypeOptions = signal<LeaveTypeResponse[]>([]);
  readonly searchResults = signal<EmployeeResponse[]>([]);
  readonly balance = signal<LeaveBalanceDto | null>(null);
  readonly balanceLoading = signal(false);
  readonly workingDays = signal<number | null>(null);
  readonly workingDaysLoading = signal(false);

  private readonly search$ = new Subject<string>();

  readonly canSubmit = computed(() => this.form.valid && !this.saving());

  ngOnInit(): void {
    this.leaveTypes.getAll({ pageSize: 100, isActive: true }).subscribe({
      next: (res) => {
        if (res.success && res.data) this.leaveTypeOptions.set(res.data.items);
      },
    });

    this.search$
      .pipe(debounceTime(300), distinctUntilChanged())
      .subscribe((term) => this.runSearch(term));

    this.form.valueChanges
      .pipe(debounceTime(300))
      .subscribe(() => {
        this.refreshBalance();
        this.refreshWorkingDays();
      });
  }

  onEmployeeSearch(term: string): void {
    this.form.patchValue({ employeeLabel: term, employeeId: 0 });
    this.balance.set(null);
    this.search$.next(term.trim());
  }

  private runSearch(term: string): void {
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
    this.refreshBalance();
  }

  private refreshBalance(): void {
    const employeeId = this.form.value.employeeId ?? 0;
    const leaveTypeId = this.form.value.leaveTypeId ?? 0;
    if (!employeeId || !leaveTypeId) {
      this.balance.set(null);
      return;
    }
    const year = new Date().getFullYear();
    this.balanceLoading.set(true);
    this.allotments.getBalance(employeeId, leaveTypeId, year).subscribe({
      next: (res) => {
        this.balanceLoading.set(false);
        this.balance.set(res.success && res.data ? res.data : null);
      },
      error: () => {
        this.balanceLoading.set(false);
        this.balance.set(null);
      },
    });
  }

  private refreshWorkingDays(): void {
    const employeeId = this.form.value.employeeId ?? 0;
    const fromDate = this.form.value.fromDate ?? '';
    const toDate = this.form.value.toDate ?? '';
    if (!employeeId || !fromDate || !toDate || toDate < fromDate) {
      this.workingDays.set(null);
      return;
    }
    this.workingDaysLoading.set(true);
    this.applications.computeWorkingDays(employeeId, fromDate, toDate).subscribe({
      next: (res) => {
        this.workingDaysLoading.set(false);
        if (res.success && res.data) {
          this.workingDays.set(res.data.workingDays);
        }
      },
      error: () => {
        // Backend may not expose this — fall back to a naive total-days count.
        this.workingDaysLoading.set(false);
        const days = totalDaysBetween(fromDate, toDate);
        this.workingDays.set(days);
      },
    });
  }

  submit(): void {
    if (!this.canSubmit()) {
      this.form.markAllAsTouched();
      return;
    }
    const raw = this.form.getRawValue();
    if (raw.toDate < raw.fromDate) {
      this.toast.error('To date cannot be before From date.');
      return;
    }

    this.saving.set(true);
    this.applications
      .create({
        employeeId: raw.employeeId,
        leaveTypeId: raw.leaveTypeId,
        fromDate: raw.fromDate,
        toDate: raw.toDate,
        reason: raw.reason.trim(),
      })
      .subscribe({
        next: (res) => {
          this.saving.set(false);
          if (res.success && res.data) {
            this.toast.success('Leave application submitted.');
            this.router.navigate(['/leave/applications']);
          } else {
            this.toast.error(res.message || 'Failed to submit application.');
          }
        },
        error: (err: HttpErrorResponse) => {
          this.saving.set(false);
          this.toast.error(err.error?.message || 'Failed to submit application.');
        },
      });
  }

  hasError(field: keyof typeof this.form.controls, error: string): boolean {
    const ctrl = this.form.controls[field];
    return ctrl.touched && ctrl.hasError(error);
  }

  selectedLeaveType(): LeaveTypeResponse | null {
    const id = this.form.value.leaveTypeId;
    return this.leaveTypeOptions().find((t) => t.id === id) ?? null;
  }
}

function totalDaysBetween(from: string, to: string): number {
  const a = new Date(from);
  const b = new Date(to);
  if (Number.isNaN(a.getTime()) || Number.isNaN(b.getTime())) return 0;
  const ms = b.getTime() - a.getTime();
  return Math.max(0, Math.round(ms / (1000 * 60 * 60 * 24)) + 1);
}

import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, map, of } from 'rxjs';

import { environment } from '../../../../environments/environment';
import {
  ApiResponse,
  PagedResult,
  toPagedResponse,
} from '../../../core/models/api-response.model';
import {
  AttendanceRow,
  AttendanceSummary,
  DepartmentHeadcount,
  EmployeeListRow,
  LeaveApplicationRow,
  RecentLeaveRow,
  RecentSalaryRow,
  SalaryCalculationRow,
} from '../models/dashboard.models';

/**
 * Aggregates the dashboard's data sources.
 *
 * Each call returns a fresh observable and swallows individual failures so
 * that a single dead endpoint can't blank out the whole dashboard. Empty
 * results render as zeros / empty tables.
 */
@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly http = inject(HttpClient);
  private readonly base = environment.apiUrl;

  /**
   * Total active employee headcount.
   * We rely on the paged response's `totalCount` so we don't transfer the
   * full payload when we only need the count.
   */
  getEmployeeCount(): Observable<number> {
    const params = new HttpParams().set('pageNumber', '1').set('pageSize', '1');
    return this.http
      .get<ApiResponse<EmployeeListRow[] | PagedResult<EmployeeListRow>>>(`${this.base}/employees`, { params })
      .pipe(
        map((res) => toPagedResponse<EmployeeListRow>(res)),
        map((res) => (res.success && res.data ? res.data.totalCount : 0)),
        catchError(() => of(0)),
      );
  }

  /** Today's attendance summary, derived client-side from the records list. */
  getTodayAttendanceSummary(): Observable<AttendanceSummary> {
    const today = todayIsoDate();
    const params = new HttpParams()
      .set('fromDate', today)
      .set('toDate', today)
      .set('pageNumber', '1')
      .set('pageSize', '500');

    return this.http
      .get<ApiResponse<AttendanceRow[] | PagedResult<AttendanceRow>>>(`${this.base}/attendance`, { params })
      .pipe(
        map((res) => toPagedResponse<AttendanceRow>(res)),
        map((res) => buildAttendanceSummary(res.success && res.data ? res.data.items : [])),
        catchError(() => of(emptyAttendanceSummary())),
      );
  }

  /** Recent pending leave applications, newest first. */
  getPendingLeaves(pageSize = 10): Observable<RecentLeaveRow[]> {
    const params = new HttpParams()
      .set('status', 'Pending')
      .set('pageNumber', '1')
      .set('pageSize', String(pageSize));

    return this.http
      .get<ApiResponse<LeaveApplicationRow[] | PagedResult<LeaveApplicationRow>>>(
        `${this.base}/leave-applications`,
        { params },
      )
      .pipe(
        map((res) => toPagedResponse<LeaveApplicationRow>(res)),
        map((res) =>
          res.success && res.data
            ? res.data.items.map(toRecentLeaveRow)
            : [],
        ),
        catchError(() => of<RecentLeaveRow[]>([])),
      );
  }

  /** Pending leave count — used for the stat card. */
  getPendingLeavesCount(): Observable<number> {
    const params = new HttpParams()
      .set('status', 'Pending')
      .set('pageNumber', '1')
      .set('pageSize', '1');

    return this.http
      .get<ApiResponse<LeaveApplicationRow[] | PagedResult<LeaveApplicationRow>>>(
        `${this.base}/leave-applications`,
        { params },
      )
      .pipe(
        map((res) => toPagedResponse<LeaveApplicationRow>(res)),
        map((res) => (res.success && res.data ? res.data.totalCount : 0)),
        catchError(() => of(0)),
      );
  }

  /** Pending loan applications count — for the stat card. */
  getPendingLoansCount(): Observable<number> {
    const params = new HttpParams()
      .set('status', 'Pending')
      .set('pageNumber', '1')
      .set('pageSize', '1');

    return this.http
      .get<ApiResponse<unknown[] | PagedResult<unknown>>>(
        `${this.base}/loan-applications`,
        { params },
      )
      .pipe(
        map((res) => toPagedResponse<unknown>(res)),
        map((res) => (res.success && res.data ? res.data.totalCount : 0)),
        catchError(() => of(0)),
      );
  }

  /** Most recently created salary calculations across all employees. */
  getRecentSalaryCalculations(pageSize = 5): Observable<RecentSalaryRow[]> {
    const params = new HttpParams()
      .set('pageNumber', '1')
      .set('pageSize', String(pageSize));

    return this.http
      .get<ApiResponse<SalaryCalculationRow[] | PagedResult<SalaryCalculationRow>>>(
        `${this.base}/salary-calculations`,
        { params },
      )
      .pipe(
        map((res) => toPagedResponse<SalaryCalculationRow>(res)),
        map((res) =>
          res.success && res.data
            ? res.data.items.map(toRecentSalaryRow)
            : [],
        ),
        catchError(() => of<RecentSalaryRow[]>([])),
      );
  }

  /**
   * Department-wise headcount.
   *
   * The backend doesn't expose a dedicated aggregate endpoint, so we pull a
   * page of active employees and group client-side. For organizations with
   * more than 200 employees this should be replaced with a server-side
   * aggregation in a follow-up.
   */
  getDepartmentHeadcount(sample = 200): Observable<DepartmentHeadcount[]> {
    const params = new HttpParams()
      .set('pageNumber', '1')
      .set('pageSize', String(sample))
      .set('isActive', 'true');

    return this.http
      .get<ApiResponse<EmployeeListRow[] | PagedResult<EmployeeListRow>>>(`${this.base}/employees`, { params })
      .pipe(
        map((res) => toPagedResponse<EmployeeListRow>(res)),
        map((res) =>
          res.success && res.data ? groupByDepartment(res.data.items) : [],
        ),
        catchError(() => of<DepartmentHeadcount[]>([])),
      );
  }
}

function todayIsoDate(): string {
  const d = new Date();
  const year = d.getFullYear();
  const month = String(d.getMonth() + 1).padStart(2, '0');
  const day = String(d.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

function emptyAttendanceSummary(): AttendanceSummary {
  return { present: 0, absent: 0, late: 0, halfDay: 0, holiday: 0, weeklyOff: 0, total: 0 };
}

function buildAttendanceSummary(rows: AttendanceRow[]): AttendanceSummary {
  const sum = emptyAttendanceSummary();
  for (const row of rows) {
    sum.total++;
    switch ((row.status ?? '').toLowerCase()) {
      case 'present':    sum.present++;   break;
      case 'absent':     sum.absent++;    break;
      case 'late':       sum.late++;      break;
      case 'halfday':
      case 'half-day':
      case 'half_day':   sum.halfDay++;   break;
      case 'holiday':    sum.holiday++;   break;
      case 'weeklyoff':
      case 'weekly-off':
      case 'weekly_off': sum.weeklyOff++; break;
      default:
        if (row.isLate) sum.late++;
    }
  }
  return sum;
}

function groupByDepartment(employees: EmployeeListRow[]): DepartmentHeadcount[] {
  const counts = new Map<string, number>();
  for (const emp of employees) {
    const key = emp.departmentName?.trim() || 'Unassigned';
    counts.set(key, (counts.get(key) ?? 0) + 1);
  }
  return Array.from(counts.entries())
    .map(([departmentName, count]) => ({ departmentName, count }))
    .sort((a, b) => b.count - a.count)
    .slice(0, 10);
}

function toRecentLeaveRow(row: LeaveApplicationRow): RecentLeaveRow {
  return {
    id: row.id,
    employeeFullName: row.employeeFullName ?? '—',
    employeeCode: row.employeeCode ?? '',
    leaveTypeName: row.leaveTypeName ?? '—',
    fromDate: row.fromDate,
    toDate: row.toDate,
    fromDateFormatted: row.fromDateFormatted ?? formatDate(row.fromDate),
    toDateFormatted: row.toDateFormatted ?? formatDate(row.toDate),
    totalDays: row.totalDays,
    status: row.status,
    appliedAt: row.createdAt,
  };
}

function toRecentSalaryRow(row: SalaryCalculationRow): RecentSalaryRow {
  const month = new Date(row.year, (row.month ?? 1) - 1, 1);
  return {
    id: row.id,
    employeeFullName: row.employeeFullName ?? '—',
    employeeCode: row.employeeCode ?? '',
    monthLabel: row.monthLabel ?? formatMonth(month),
    netSalary: row.netSalary ?? 0,
    netSalaryFormatted: row.netSalaryFormatted ?? formatNumber(row.netSalary ?? 0),
    status: row.status,
    createdAt: row.createdAt,
  };
}

function formatDate(value: string | null | undefined): string {
  if (!value) return '';
  const d = new Date(value);
  if (Number.isNaN(d.getTime())) return '';
  return d.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
}

function formatMonth(d: Date): string {
  return d.toLocaleDateString('en-GB', { month: 'long', year: 'numeric' });
}

function formatNumber(value: number): string {
  return new Intl.NumberFormat('en-IN', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(value);
}

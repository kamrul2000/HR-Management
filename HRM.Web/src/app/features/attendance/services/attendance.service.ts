import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, forkJoin, map, of } from 'rxjs';

import { environment } from '../../../../environments/environment';
import {
  ApiResponse,
  PagedResult,
  toPagedResponse,
} from '../../../core/models/api-response.model';
import {
  AttendanceFilter,
  AttendanceResponse,
  BulkAttendanceDto,
  CreateAttendanceDto,
  MonthlyAttendanceSummary,
  UpdateAttendanceDto,
} from '../models/attendance.model';

@Injectable({ providedIn: 'root' })
export class AttendanceService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/attendance`;

  getAll(filter: AttendanceFilter = {}): Observable<ApiResponse<PagedResult<AttendanceResponse>>> {
    let params = new HttpParams();
    if (filter.search)       params = params.set('search', filter.search);
    if (filter.branchId)     params = params.set('branchId', String(filter.branchId));
    if (filter.departmentId) params = params.set('departmentId', String(filter.departmentId));
    if (filter.employeeId)   params = params.set('employeeId', String(filter.employeeId));
    if (filter.status)       params = params.set('status', filter.status);
    if (filter.fromDate)     params = params.set('fromDate', filter.fromDate);
    if (filter.toDate)       params = params.set('toDate', filter.toDate);
    if (filter.pageNumber)   params = params.set('pageNumber', String(filter.pageNumber));
    if (filter.pageSize)     params = params.set('pageSize',   String(filter.pageSize));
    return this.http
      .get<ApiResponse<AttendanceResponse[] | PagedResult<AttendanceResponse>>>(this.base, { params })
      .pipe(map((res) => toPagedResponse<AttendanceResponse>(res)));
  }

  getById(id: number): Observable<ApiResponse<AttendanceResponse>> {
    return this.http.get<ApiResponse<AttendanceResponse>>(`${this.base}/${id}`);
  }

  create(dto: CreateAttendanceDto): Observable<ApiResponse<AttendanceResponse>> {
    return this.http.post<ApiResponse<AttendanceResponse>>(this.base, dto);
  }

  update(id: number, dto: UpdateAttendanceDto): Observable<ApiResponse<AttendanceResponse>> {
    return this.http.put<ApiResponse<AttendanceResponse>>(`${this.base}/${id}`, dto);
  }

  delete(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.base}/${id}`);
  }

  /**
   * Submit a batch of attendance entries.
   *
   * Tries the dedicated `/bulk` endpoint first; if your backend prefers
   * per-row inserts, swap to `forkJoin(rows.map(this.create))` instead.
   */
  bulkCreate(dto: BulkAttendanceDto): Observable<ApiResponse<AttendanceResponse[]>> {
    return this.http.post<ApiResponse<AttendanceResponse[]>>(`${this.base}/bulk`, dto);
  }

  /** Per-row submission used as a fallback when /bulk isn't available. */
  bulkCreateSequential(dto: BulkAttendanceDto): Observable<{ success: number; failed: number }> {
    if (dto.rows.length === 0) return of({ success: 0, failed: 0 });
    return forkJoin(
      dto.rows.map((row) =>
        this.create({
          employeeId: row.employeeId,
          dutySlotId: dto.dutySlotId,
          attendanceDate: dto.attendanceDate,
          punchInTime: row.punchInTime,
          punchOutTime: row.punchOutTime,
          status: row.status,
          remarks: row.remarks,
        }),
      ),
    ).pipe(
      map((results) => ({
        success: results.filter((r) => r.success).length,
        failed: results.filter((r) => !r.success).length,
      })),
    );
  }

  getMonthlySummary(employeeId: number, year: number, month: number):
    Observable<ApiResponse<MonthlyAttendanceSummary>> {
    return this.http.get<ApiResponse<MonthlyAttendanceSummary>>(
      `${this.base}/summary/${employeeId}/${year}/${month}`,
    );
  }
}

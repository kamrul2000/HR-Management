import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';

import { environment } from '../../../../environments/environment';
import {
  ApiResponse,
  PagedResult,
  toPagedResponse,
} from '../../../core/models/api-response.model';
import {
  ApproveLeaveApplicationDto,
  CancelLeaveApplicationDto,
  CreateLeaveApplicationDto,
  LeaveApplicationFilter,
  LeaveApplicationResponse,
  RejectLeaveApplicationDto,
  WorkingDaysResult,
} from '../models/leave-application.model';

@Injectable({ providedIn: 'root' })
export class LeaveApplicationService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/leave-applications`;

  getAll(filter: LeaveApplicationFilter = {}):
    Observable<ApiResponse<PagedResult<LeaveApplicationResponse>>> {
    let params = new HttpParams();
    if (filter.search)      params = params.set('search', filter.search);
    if (filter.employeeId)  params = params.set('employeeId', String(filter.employeeId));
    if (filter.leaveTypeId) params = params.set('leaveTypeId', String(filter.leaveTypeId));
    if (filter.branchId)    params = params.set('branchId', String(filter.branchId));
    if (filter.status)      params = params.set('status', filter.status);
    if (filter.fromDate)    params = params.set('fromDate', filter.fromDate);
    if (filter.toDate)      params = params.set('toDate', filter.toDate);
    if (filter.pageNumber)  params = params.set('pageNumber', String(filter.pageNumber));
    if (filter.pageSize)    params = params.set('pageSize', String(filter.pageSize));
    return this.http
      .get<ApiResponse<LeaveApplicationResponse[] | PagedResult<LeaveApplicationResponse>>>(
        this.base, { params })
      .pipe(map((res) => toPagedResponse<LeaveApplicationResponse>(res)));
  }

  getById(id: number): Observable<ApiResponse<LeaveApplicationResponse>> {
    return this.http.get<ApiResponse<LeaveApplicationResponse>>(`${this.base}/${id}`);
  }

  create(dto: CreateLeaveApplicationDto): Observable<ApiResponse<LeaveApplicationResponse>> {
    return this.http.post<ApiResponse<LeaveApplicationResponse>>(this.base, dto);
  }

  approve(id: number, dto: ApproveLeaveApplicationDto):
    Observable<ApiResponse<LeaveApplicationResponse>> {
    return this.http.put<ApiResponse<LeaveApplicationResponse>>(
      `${this.base}/${id}/approve`, dto);
  }

  reject(id: number, dto: RejectLeaveApplicationDto):
    Observable<ApiResponse<LeaveApplicationResponse>> {
    return this.http.put<ApiResponse<LeaveApplicationResponse>>(
      `${this.base}/${id}/reject`, dto);
  }

  cancel(id: number, dto: CancelLeaveApplicationDto):
    Observable<ApiResponse<LeaveApplicationResponse>> {
    return this.http.put<ApiResponse<LeaveApplicationResponse>>(
      `${this.base}/${id}/cancel`, dto);
  }

  uploadAttachment(id: number, file: File):
    Observable<ApiResponse<LeaveApplicationResponse>> {
    const fd = new FormData();
    fd.append('file', file);
    return this.http.post<ApiResponse<LeaveApplicationResponse>>(
      `${this.base}/${id}/attachment`, fd);
  }

  /**
   * Compute "effective" working days between two dates excluding holidays and
   * off-days for the employee's branch. Falls back to a client-side
   * approximation if the backend doesn't expose the helper.
   */
  computeWorkingDays(employeeId: number, fromDate: string, toDate: string):
    Observable<ApiResponse<WorkingDaysResult>> {
    const params = new HttpParams()
      .set('employeeId', String(employeeId))
      .set('fromDate', fromDate)
      .set('toDate', toDate);
    return this.http.get<ApiResponse<WorkingDaysResult>>(
      `${this.base}/working-days`, { params });
  }
}

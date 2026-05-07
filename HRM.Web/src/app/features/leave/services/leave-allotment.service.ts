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
  BulkAllotmentDto,
  BulkAllotmentResult,
  CreateLeaveAllotmentDto,
  LeaveAllotmentFilter,
  LeaveAllotmentResponse,
  LeaveBalanceDto,
  UpdateLeaveAllotmentDto,
} from '../models/leave-allotment.model';

@Injectable({ providedIn: 'root' })
export class LeaveAllotmentService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/leave-allotments`;

  getAll(filter: LeaveAllotmentFilter = {}):
    Observable<ApiResponse<PagedResult<LeaveAllotmentResponse>>> {
    let params = new HttpParams();
    if (filter.employeeId)  params = params.set('employeeId',  String(filter.employeeId));
    if (filter.leaveTypeId) params = params.set('leaveTypeId', String(filter.leaveTypeId));
    if (filter.year)        params = params.set('year',        String(filter.year));
    if (filter.branchId)    params = params.set('branchId',    String(filter.branchId));
    if (filter.pageNumber)  params = params.set('pageNumber',  String(filter.pageNumber));
    if (filter.pageSize)    params = params.set('pageSize',    String(filter.pageSize));
    return this.http
      .get<ApiResponse<LeaveAllotmentResponse[] | PagedResult<LeaveAllotmentResponse>>>(
        this.base, { params })
      .pipe(map((res) => toPagedResponse<LeaveAllotmentResponse>(res)));
  }

  getById(id: number): Observable<ApiResponse<LeaveAllotmentResponse>> {
    return this.http.get<ApiResponse<LeaveAllotmentResponse>>(`${this.base}/${id}`);
  }

  create(dto: CreateLeaveAllotmentDto): Observable<ApiResponse<LeaveAllotmentResponse>> {
    return this.http.post<ApiResponse<LeaveAllotmentResponse>>(this.base, dto);
  }

  update(id: number, dto: UpdateLeaveAllotmentDto): Observable<ApiResponse<LeaveAllotmentResponse>> {
    return this.http.put<ApiResponse<LeaveAllotmentResponse>>(`${this.base}/${id}`, dto);
  }

  delete(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.base}/${id}`);
  }

  bulkCreate(dto: BulkAllotmentDto): Observable<ApiResponse<BulkAllotmentResult>> {
    return this.http.post<ApiResponse<BulkAllotmentResult>>(`${this.base}/bulk`, dto);
  }

  /** Look up an employee's current balance for a specific leave type & year. */
  getBalance(employeeId: number, leaveTypeId: number, year: number):
    Observable<ApiResponse<LeaveBalanceDto>> {
    const params = new HttpParams()
      .set('employeeId', String(employeeId))
      .set('leaveTypeId', String(leaveTypeId))
      .set('year', String(year));
    return this.http.get<ApiResponse<LeaveBalanceDto>>(`${this.base}/balance`, { params });
  }
}

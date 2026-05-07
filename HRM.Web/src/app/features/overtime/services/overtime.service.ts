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
  ApproveOvertimeDto,
  CreateOvertimeDto,
  OvertimeFilter,
  OvertimeResponse,
  OvertimeSummary,
  RejectOvertimeDto,
} from '../models/overtime.model';

@Injectable({ providedIn: 'root' })
export class OvertimeService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/overtime`;

  getAll(filter: OvertimeFilter = {}): Observable<ApiResponse<PagedResult<OvertimeResponse>>> {
    let params = new HttpParams();
    if (filter.search)        params = params.set('search', filter.search);
    if (filter.employeeId)    params = params.set('employeeId', String(filter.employeeId));
    if (filter.branchId)      params = params.set('branchId', String(filter.branchId));
    if (filter.status)        params = params.set('status', filter.status);
    if (filter.overtimeType)  params = params.set('overtimeType', filter.overtimeType);
    if (filter.year)          params = params.set('year', String(filter.year));
    if (filter.month)         params = params.set('month', String(filter.month));
    if (filter.pageNumber)    params = params.set('pageNumber', String(filter.pageNumber));
    if (filter.pageSize)      params = params.set('pageSize', String(filter.pageSize));
    return this.http
      .get<ApiResponse<OvertimeResponse[] | PagedResult<OvertimeResponse>>>(this.base, { params })
      .pipe(map((res) => toPagedResponse<OvertimeResponse>(res)));
  }

  getById(id: number): Observable<ApiResponse<OvertimeResponse>> {
    return this.http.get<ApiResponse<OvertimeResponse>>(`${this.base}/${id}`);
  }

  create(dto: CreateOvertimeDto): Observable<ApiResponse<OvertimeResponse>> {
    return this.http.post<ApiResponse<OvertimeResponse>>(this.base, dto);
  }

  approve(id: number, dto: ApproveOvertimeDto): Observable<ApiResponse<OvertimeResponse>> {
    return this.http.put<ApiResponse<OvertimeResponse>>(`${this.base}/${id}/approve`, dto);
  }

  reject(id: number, dto: RejectOvertimeDto): Observable<ApiResponse<OvertimeResponse>> {
    return this.http.put<ApiResponse<OvertimeResponse>>(`${this.base}/${id}/reject`, dto);
  }

  delete(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.base}/${id}`);
  }

  getMonthlySummary(year: number, month: number, branchId?: number):
    Observable<ApiResponse<OvertimeSummary[]>> {
    let params = new HttpParams()
      .set('year', String(year))
      .set('month', String(month));
    if (branchId) params = params.set('branchId', String(branchId));
    return this.http.get<ApiResponse<OvertimeSummary[]>>(`${this.base}/summary`, { params });
  }
}

import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';

import { environment } from '../../../../environments/environment';
import {
  ApiResponse,
  BulkCreateResult,
  PagedResult,
  toPagedResponse,
} from '../../../core/models/api-response.model';
import {
  BulkRunSalaryDto,
  RunSalaryCalculationDto,
  SalaryCalculationFilter,
  SalaryCalculationResponse,
} from '../models/salary-calculation.model';

@Injectable({ providedIn: 'root' })
export class SalaryCalculationService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/salary-calculations`;

  getAll(filter: SalaryCalculationFilter = {}):
    Observable<ApiResponse<PagedResult<SalaryCalculationResponse>>> {
    let params = new HttpParams();
    if (filter.employeeId)   params = params.set('employeeId', String(filter.employeeId));
    if (filter.branchId)     params = params.set('branchId',   String(filter.branchId));
    if (filter.departmentId) params = params.set('departmentId', String(filter.departmentId));
    if (filter.year)         params = params.set('year', String(filter.year));
    if (filter.month)        params = params.set('month', String(filter.month));
    if (filter.status)       params = params.set('status', filter.status);
    if (filter.pageNumber)   params = params.set('pageNumber', String(filter.pageNumber));
    if (filter.pageSize)     params = params.set('pageSize', String(filter.pageSize));
    return this.http
      .get<ApiResponse<SalaryCalculationResponse[] | PagedResult<SalaryCalculationResponse>>>(
        this.base, { params })
      .pipe(map((res) => toPagedResponse<SalaryCalculationResponse>(res)));
  }

  getById(id: number): Observable<ApiResponse<SalaryCalculationResponse>> {
    return this.http.get<ApiResponse<SalaryCalculationResponse>>(`${this.base}/${id}`);
  }

  calculate(dto: RunSalaryCalculationDto): Observable<ApiResponse<SalaryCalculationResponse>> {
    return this.http.post<ApiResponse<SalaryCalculationResponse>>(`${this.base}/calculate`, dto);
  }

  bulkCalculate(dto: BulkRunSalaryDto): Observable<ApiResponse<BulkCreateResult>> {
    return this.http.post<ApiResponse<BulkCreateResult>>(`${this.base}/bulk-calculate`, dto);
  }

  finalize(id: number): Observable<ApiResponse<SalaryCalculationResponse>> {
    return this.http.put<ApiResponse<SalaryCalculationResponse>>(`${this.base}/${id}/finalize`, {});
  }

  cancel(id: number, reason: string): Observable<ApiResponse<SalaryCalculationResponse>> {
    const params = new HttpParams().set('reason', reason);
    return this.http.put<ApiResponse<SalaryCalculationResponse>>(
      `${this.base}/${id}/cancel`, {}, { params });
  }
}

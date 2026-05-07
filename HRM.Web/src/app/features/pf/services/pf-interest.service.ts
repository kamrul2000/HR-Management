import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { ApiResponse, BulkCreateResult } from '../../../core/models/api-response.model';
import {
  BulkComputePfInterestDto,
  ComputePfInterestDto,
  CreatePfInterestRateDto,
  EmployeePfInterestResponse,
  PfInterestRateResponse,
  PfInterestReport,
} from '../models/pf-interest.model';

@Injectable({ providedIn: 'root' })
export class PfInterestService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/pf-interest`;

  // Rates
  getAllRates(): Observable<ApiResponse<PfInterestRateResponse[]>> {
    return this.http.get<ApiResponse<PfInterestRateResponse[]>>(`${this.base}/rates`);
  }

  getRateByFiscalYear(fy: string): Observable<ApiResponse<PfInterestRateResponse>> {
    return this.http.get<ApiResponse<PfInterestRateResponse>>(`${this.base}/rates/${fy}`);
  }

  createRate(dto: CreatePfInterestRateDto): Observable<ApiResponse<PfInterestRateResponse>> {
    return this.http.post<ApiResponse<PfInterestRateResponse>>(`${this.base}/rates`, dto);
  }

  // Computation
  compute(dto: ComputePfInterestDto): Observable<ApiResponse<EmployeePfInterestResponse>> {
    return this.http.post<ApiResponse<EmployeePfInterestResponse>>(`${this.base}/compute`, dto);
  }

  bulkCompute(dto: BulkComputePfInterestDto): Observable<ApiResponse<BulkCreateResult>> {
    return this.http.post<ApiResponse<BulkCreateResult>>(`${this.base}/bulk-compute`, dto);
  }

  getById(id: number): Observable<ApiResponse<EmployeePfInterestResponse>> {
    return this.http.get<ApiResponse<EmployeePfInterestResponse>>(`${this.base}/${id}`);
  }

  getByEmployee(employeeId: number): Observable<ApiResponse<EmployeePfInterestResponse[]>> {
    return this.http.get<ApiResponse<EmployeePfInterestResponse[]>>(`${this.base}/by-employee/${employeeId}`);
  }

  getReport(fy: string, branchId?: number | null): Observable<ApiResponse<PfInterestReport>> {
    let url = `${this.base}/report/${fy}`;
    if (branchId) url += `?branchId=${branchId}`;
    return this.http.get<ApiResponse<PfInterestReport>>(url);
  }
}

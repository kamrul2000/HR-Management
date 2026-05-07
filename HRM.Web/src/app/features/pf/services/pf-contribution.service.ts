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
  CreatePfRuleDto,
  EmployeePfContributionResponse,
  PfContributionFilter,
  PfMonthlyReport,
  PfRuleResponse,
  UpdatePfRuleDto,
} from '../models/pf-contribution.model';

@Injectable({ providedIn: 'root' })
export class PfContributionService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/pf`;

  // Rules
  getAllRules(): Observable<ApiResponse<PfRuleResponse[]>> {
    return this.http.get<ApiResponse<PfRuleResponse[]>>(`${this.base}/rules`);
  }

  getActiveRule(): Observable<ApiResponse<PfRuleResponse>> {
    return this.http.get<ApiResponse<PfRuleResponse>>(`${this.base}/rules/active`);
  }

  getRuleById(id: number): Observable<ApiResponse<PfRuleResponse>> {
    return this.http.get<ApiResponse<PfRuleResponse>>(`${this.base}/rules/${id}`);
  }

  createRule(dto: CreatePfRuleDto): Observable<ApiResponse<PfRuleResponse>> {
    return this.http.post<ApiResponse<PfRuleResponse>>(`${this.base}/rules`, dto);
  }

  updateRule(id: number, dto: UpdatePfRuleDto): Observable<ApiResponse<PfRuleResponse>> {
    return this.http.put<ApiResponse<PfRuleResponse>>(`${this.base}/rules/${id}`, dto);
  }

  // Contributions
  compute(employeeId: number, year: number, month: number): Observable<ApiResponse<EmployeePfContributionResponse>> {
    return this.http.post<ApiResponse<EmployeePfContributionResponse>>(
      `${this.base}/compute/${employeeId}/${year}/${month}`,
      {},
    );
  }

  bulkCompute(year: number, month: number, branchId?: number | null): Observable<ApiResponse<BulkCreateResult>> {
    let params = new HttpParams().set('year', String(year)).set('month', String(month));
    if (branchId) params = params.set('branchId', String(branchId));
    return this.http.post<ApiResponse<BulkCreateResult>>(`${this.base}/bulk-compute`, {}, { params });
  }

  getFiltered(filter: PfContributionFilter = {}): Observable<ApiResponse<PagedResult<EmployeePfContributionResponse>>> {
    let params = new HttpParams();
    if (filter.employeeId) params = params.set('employeeId', String(filter.employeeId));
    if (filter.branchId)   params = params.set('branchId',   String(filter.branchId));
    if (filter.year)       params = params.set('year',       String(filter.year));
    if (filter.month)      params = params.set('month',      String(filter.month));
    if (filter.pageNumber) params = params.set('pageNumber', String(filter.pageNumber));
    if (filter.pageSize)   params = params.set('pageSize',   String(filter.pageSize));
    return this.http
      .get<ApiResponse<EmployeePfContributionResponse[] | PagedResult<EmployeePfContributionResponse>>>(
        `${this.base}/contributions`, { params },
      )
      .pipe(map((res) => toPagedResponse<EmployeePfContributionResponse>(res)));
  }

  getContributionById(id: number): Observable<ApiResponse<EmployeePfContributionResponse>> {
    return this.http.get<ApiResponse<EmployeePfContributionResponse>>(`${this.base}/contributions/${id}`);
  }

  getByEmployee(employeeId: number, year?: number): Observable<ApiResponse<EmployeePfContributionResponse[]>> {
    let params = new HttpParams();
    if (year) params = params.set('year', String(year));
    return this.http.get<ApiResponse<EmployeePfContributionResponse[]>>(
      `${this.base}/contributions/by-employee/${employeeId}`, { params },
    );
  }

  getReport(year: number, month: number, branchId?: number | null): Observable<ApiResponse<PfMonthlyReport>> {
    let params = new HttpParams();
    if (branchId) params = params.set('branchId', String(branchId));
    return this.http.get<ApiResponse<PfMonthlyReport>>(`${this.base}/report/${year}/${month}`, { params });
  }
}

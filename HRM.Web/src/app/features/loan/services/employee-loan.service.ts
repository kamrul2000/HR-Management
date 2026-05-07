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
  CreateEmployeeLoanDto,
  EmployeeLoanFilter,
  EmployeeLoanResponse,
} from '../models/employee-loan.model';

@Injectable({ providedIn: 'root' })
export class EmployeeLoanService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/employee-loans`;

  getAll(filter: EmployeeLoanFilter = {}): Observable<ApiResponse<PagedResult<EmployeeLoanResponse>>> {
    let params = new HttpParams();
    if (filter.employeeId) params = params.set('employeeId', String(filter.employeeId));
    if (filter.branchId)   params = params.set('branchId',   String(filter.branchId));
    if (filter.status)     params = params.set('status',     filter.status);
    if (filter.loanType)   params = params.set('loanType',   filter.loanType);
    if (filter.pageNumber) params = params.set('pageNumber', String(filter.pageNumber));
    if (filter.pageSize)   params = params.set('pageSize',   String(filter.pageSize));
    return this.http
      .get<ApiResponse<EmployeeLoanResponse[] | PagedResult<EmployeeLoanResponse>>>(this.base, { params })
      .pipe(map((res) => toPagedResponse<EmployeeLoanResponse>(res)));
  }

  getById(id: number): Observable<ApiResponse<EmployeeLoanResponse>> {
    return this.http.get<ApiResponse<EmployeeLoanResponse>>(`${this.base}/${id}`);
  }

  getByEmployee(employeeId: number): Observable<ApiResponse<EmployeeLoanResponse>> {
    return this.http.get<ApiResponse<EmployeeLoanResponse>>(`${this.base}/by-employee/${employeeId}`);
  }

  create(dto: CreateEmployeeLoanDto): Observable<ApiResponse<EmployeeLoanResponse>> {
    return this.http.post<ApiResponse<EmployeeLoanResponse>>(this.base, dto);
  }

  complete(id: number): Observable<ApiResponse<EmployeeLoanResponse>> {
    return this.http.put<ApiResponse<EmployeeLoanResponse>>(`${this.base}/${id}/complete`, {});
  }

  default(id: number, reason: string): Observable<ApiResponse<EmployeeLoanResponse>> {
    const params = new HttpParams().set('reason', reason);
    return this.http.put<ApiResponse<EmployeeLoanResponse>>(`${this.base}/${id}/default`, {}, { params });
  }

  cancel(id: number, reason: string): Observable<ApiResponse<EmployeeLoanResponse>> {
    const params = new HttpParams().set('reason', reason);
    return this.http.put<ApiResponse<EmployeeLoanResponse>>(`${this.base}/${id}/cancel`, {}, { params });
  }
}

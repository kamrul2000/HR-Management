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
  CancelLoanApplicationDto,
  CreateLoanApplicationDto,
  LoanApplicationFilter,
  LoanApplicationResponse,
} from '../models/loan-application.model';

@Injectable({ providedIn: 'root' })
export class LoanApplicationService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/loan-applications`;

  getAll(filter: LoanApplicationFilter = {}): Observable<ApiResponse<PagedResult<LoanApplicationResponse>>> {
    let params = new HttpParams();
    if (filter.employeeId)  params = params.set('employeeId',  String(filter.employeeId));
    if (filter.branchId)    params = params.set('branchId',    String(filter.branchId));
    if (filter.loanType)    params = params.set('loanType',    filter.loanType);
    if (filter.status)      params = params.set('status',      filter.status);
    if (filter.fromDate)    params = params.set('fromDate',    filter.fromDate);
    if (filter.toDate)      params = params.set('toDate',      filter.toDate);
    if (filter.pageNumber)  params = params.set('pageNumber',  String(filter.pageNumber));
    if (filter.pageSize)    params = params.set('pageSize',    String(filter.pageSize));
    return this.http
      .get<ApiResponse<LoanApplicationResponse[] | PagedResult<LoanApplicationResponse>>>(this.base, { params })
      .pipe(map((res) => toPagedResponse<LoanApplicationResponse>(res)));
  }

  getById(id: number): Observable<ApiResponse<LoanApplicationResponse>> {
    return this.http.get<ApiResponse<LoanApplicationResponse>>(`${this.base}/${id}`);
  }

  getByEmployee(employeeId: number): Observable<ApiResponse<LoanApplicationResponse[]>> {
    return this.http.get<ApiResponse<LoanApplicationResponse[]>>(`${this.base}/by-employee/${employeeId}`);
  }

  create(dto: CreateLoanApplicationDto): Observable<ApiResponse<LoanApplicationResponse>> {
    return this.http.post<ApiResponse<LoanApplicationResponse>>(this.base, dto);
  }

  cancel(id: number, dto: CancelLoanApplicationDto): Observable<ApiResponse<LoanApplicationResponse>> {
    return this.http.put<ApiResponse<LoanApplicationResponse>>(`${this.base}/${id}/cancel`, dto);
  }

  delete(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.base}/${id}`);
  }

  uploadAttachment(id: number, file: File): Observable<ApiResponse<LoanApplicationResponse>> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<ApiResponse<LoanApplicationResponse>>(`${this.base}/${id}/attachment`, form);
  }
}

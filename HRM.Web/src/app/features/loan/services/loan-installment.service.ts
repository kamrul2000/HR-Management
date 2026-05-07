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
  InstallmentFilter,
  LoanInstallmentResponse,
  PendingInstallment,
  ProcessInstallmentDto,
  SkipInstallmentDto,
} from '../models/loan-installment.model';

@Injectable({ providedIn: 'root' })
export class LoanInstallmentService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/loan-installments`;

  getAll(filter: InstallmentFilter = {}): Observable<ApiResponse<PagedResult<LoanInstallmentResponse>>> {
    let params = new HttpParams();
    if (filter.employeeId)     params = params.set('employeeId',     String(filter.employeeId));
    if (filter.employeeLoanId) params = params.set('employeeLoanId', String(filter.employeeLoanId));
    if (filter.status)         params = params.set('status',         filter.status);
    if (filter.dueMonth)       params = params.set('dueMonth',       String(filter.dueMonth));
    if (filter.dueYear)        params = params.set('dueYear',        String(filter.dueYear));
    if (filter.pageNumber)     params = params.set('pageNumber',     String(filter.pageNumber));
    if (filter.pageSize)       params = params.set('pageSize',       String(filter.pageSize));
    return this.http
      .get<ApiResponse<LoanInstallmentResponse[] | PagedResult<LoanInstallmentResponse>>>(this.base, { params })
      .pipe(map((res) => toPagedResponse<LoanInstallmentResponse>(res)));
  }

  getById(id: number): Observable<ApiResponse<LoanInstallmentResponse>> {
    return this.http.get<ApiResponse<LoanInstallmentResponse>>(`${this.base}/${id}`);
  }

  getByLoan(employeeLoanId: number): Observable<ApiResponse<LoanInstallmentResponse[]>> {
    return this.http.get<ApiResponse<LoanInstallmentResponse[]>>(`${this.base}/by-loan/${employeeLoanId}`);
  }

  getPending(employeeId: number, year: number, month: number): Observable<ApiResponse<PendingInstallment | null>> {
    return this.http.get<ApiResponse<PendingInstallment | null>>(`${this.base}/pending/${employeeId}/${year}/${month}`);
  }

  pay(id: number, dto: ProcessInstallmentDto): Observable<ApiResponse<LoanInstallmentResponse>> {
    return this.http.put<ApiResponse<LoanInstallmentResponse>>(`${this.base}/${id}/pay`, dto);
  }

  skip(id: number, dto: SkipInstallmentDto): Observable<ApiResponse<LoanInstallmentResponse>> {
    return this.http.put<ApiResponse<LoanInstallmentResponse>>(`${this.base}/${id}/skip`, dto);
  }

  reinstate(id: number): Observable<ApiResponse<LoanInstallmentResponse>> {
    return this.http.put<ApiResponse<LoanInstallmentResponse>>(`${this.base}/${id}/reinstate`, {});
  }

  markOverdue(year: number, month: number): Observable<ApiResponse<{ updated: number }>> {
    const params = new HttpParams().set('year', String(year)).set('month', String(month));
    return this.http.post<ApiResponse<{ updated: number }>>(`${this.base}/mark-overdue`, {}, { params });
  }
}

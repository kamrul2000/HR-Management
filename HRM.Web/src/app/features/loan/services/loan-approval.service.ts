import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../core/models/api-response.model';
import {
  CreateLoanApprovalDto,
  LoanApprovalResponse,
} from '../models/loan-approval.model';

@Injectable({ providedIn: 'root' })
export class LoanApprovalService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/loan-approvals`;

  getAll(): Observable<ApiResponse<LoanApprovalResponse[]>> {
    return this.http.get<ApiResponse<LoanApprovalResponse[]>>(this.base);
  }

  getById(id: number): Observable<ApiResponse<LoanApprovalResponse>> {
    return this.http.get<ApiResponse<LoanApprovalResponse>>(`${this.base}/${id}`);
  }

  getByApplication(loanApplicationId: number): Observable<ApiResponse<LoanApprovalResponse>> {
    return this.http.get<ApiResponse<LoanApprovalResponse>>(`${this.base}/by-application/${loanApplicationId}`);
  }

  process(dto: CreateLoanApprovalDto): Observable<ApiResponse<LoanApprovalResponse>> {
    return this.http.post<ApiResponse<LoanApprovalResponse>>(this.base, dto);
  }
}

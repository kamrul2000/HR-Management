import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../core/models/api-response.model';
import {
  ComputeGratuityDto,
  GratuityCalculationResponse,
  GratuityReport,
} from '../models/gratuity-calculation.model';

@Injectable({ providedIn: 'root' })
export class GratuityCalculationService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/gratuity-calculations`;

  getAll(): Observable<ApiResponse<GratuityCalculationResponse[]>> {
    return this.http.get<ApiResponse<GratuityCalculationResponse[]>>(this.base);
  }

  getById(id: number): Observable<ApiResponse<GratuityCalculationResponse>> {
    return this.http.get<ApiResponse<GratuityCalculationResponse>>(`${this.base}/${id}`);
  }

  getByEmployee(employeeId: number): Observable<ApiResponse<GratuityCalculationResponse>> {
    return this.http.get<ApiResponse<GratuityCalculationResponse>>(`${this.base}/by-employee/${employeeId}`);
  }

  getReport(branchId?: number | null, status?: string): Observable<ApiResponse<GratuityReport>> {
    let params = new HttpParams();
    if (branchId) params = params.set('branchId', String(branchId));
    if (status)   params = params.set('status', status);
    return this.http.get<ApiResponse<GratuityReport>>(`${this.base}/report`, { params });
  }

  compute(dto: ComputeGratuityDto): Observable<ApiResponse<GratuityCalculationResponse>> {
    return this.http.post<ApiResponse<GratuityCalculationResponse>>(`${this.base}/compute`, dto);
  }

  finalize(id: number): Observable<ApiResponse<GratuityCalculationResponse>> {
    return this.http.put<ApiResponse<GratuityCalculationResponse>>(`${this.base}/${id}/finalize`, {});
  }

  cancel(id: number, reason: string): Observable<ApiResponse<GratuityCalculationResponse>> {
    const params = new HttpParams().set('reason', reason);
    return this.http.put<ApiResponse<GratuityCalculationResponse>>(`${this.base}/${id}/cancel`, {}, { params });
  }
}

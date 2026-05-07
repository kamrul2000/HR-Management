import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../core/models/api-response.model';
import {
  CreateTaxExclusionDto,
  TaxExclusionCheck,
  TaxExclusionResponse,
  UpdateTaxExclusionDto,
} from '../models/tax-exclusion.model';

@Injectable({ providedIn: 'root' })
export class TaxExclusionService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/tax-exclusions`;

  getAll(): Observable<ApiResponse<TaxExclusionResponse[]>> {
    return this.http.get<ApiResponse<TaxExclusionResponse[]>>(this.base);
  }

  getById(id: number): Observable<ApiResponse<TaxExclusionResponse>> {
    return this.http.get<ApiResponse<TaxExclusionResponse>>(`${this.base}/${id}`);
  }

  getByEmployee(employeeId: number): Observable<ApiResponse<TaxExclusionResponse[]>> {
    return this.http.get<ApiResponse<TaxExclusionResponse[]>>(`${this.base}/by-employee/${employeeId}`);
  }

  check(employeeId: number): Observable<ApiResponse<TaxExclusionCheck>> {
    return this.http.get<ApiResponse<TaxExclusionCheck>>(`${this.base}/check/${employeeId}`);
  }

  create(dto: CreateTaxExclusionDto): Observable<ApiResponse<TaxExclusionResponse>> {
    return this.http.post<ApiResponse<TaxExclusionResponse>>(this.base, dto);
  }

  update(id: number, dto: UpdateTaxExclusionDto): Observable<ApiResponse<TaxExclusionResponse>> {
    return this.http.put<ApiResponse<TaxExclusionResponse>>(`${this.base}/${id}`, dto);
  }

  delete(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.base}/${id}`);
  }

  uploadAttachment(id: number, file: File): Observable<ApiResponse<TaxExclusionResponse>> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<ApiResponse<TaxExclusionResponse>>(`${this.base}/${id}/attachment`, form);
  }
}

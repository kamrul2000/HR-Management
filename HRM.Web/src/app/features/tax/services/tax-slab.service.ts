import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../core/models/api-response.model';
import {
  ComputeTaxDto,
  CreateTaxSlabConfigDto,
  TaxComputationResult,
  TaxSlabConfigResponse,
  UpdateTaxSlabConfigDto,
} from '../models/tax-slab.model';

@Injectable({ providedIn: 'root' })
export class TaxSlabService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/tax-slabs`;

  getAll(): Observable<ApiResponse<TaxSlabConfigResponse[]>> {
    return this.http.get<ApiResponse<TaxSlabConfigResponse[]>>(this.base);
  }

  getActive(): Observable<ApiResponse<TaxSlabConfigResponse>> {
    return this.http.get<ApiResponse<TaxSlabConfigResponse>>(`${this.base}/active`);
  }

  getById(id: number): Observable<ApiResponse<TaxSlabConfigResponse>> {
    return this.http.get<ApiResponse<TaxSlabConfigResponse>>(`${this.base}/${id}`);
  }

  getByFiscalYear(fy: string): Observable<ApiResponse<TaxSlabConfigResponse>> {
    return this.http.get<ApiResponse<TaxSlabConfigResponse>>(`${this.base}/fiscal-year/${fy}`);
  }

  create(dto: CreateTaxSlabConfigDto): Observable<ApiResponse<TaxSlabConfigResponse>> {
    return this.http.post<ApiResponse<TaxSlabConfigResponse>>(this.base, dto);
  }

  update(id: number, dto: UpdateTaxSlabConfigDto): Observable<ApiResponse<TaxSlabConfigResponse>> {
    return this.http.put<ApiResponse<TaxSlabConfigResponse>>(`${this.base}/${id}`, dto);
  }

  delete(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.base}/${id}`);
  }

  compute(dto: ComputeTaxDto): Observable<ApiResponse<TaxComputationResult>> {
    return this.http.post<ApiResponse<TaxComputationResult>>(`${this.base}/compute`, dto);
  }
}

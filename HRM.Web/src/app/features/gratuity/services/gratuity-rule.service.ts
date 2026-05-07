import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../core/models/api-response.model';
import {
  CreateGratuityRuleDto,
  GratuityPreviewDto,
  GratuityPreviewResult,
  GratuityRuleResponse,
  UpdateGratuityRuleDto,
} from '../models/gratuity-rule.model';

@Injectable({ providedIn: 'root' })
export class GratuityRuleService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/gratuity-rules`;

  getAll(): Observable<ApiResponse<GratuityRuleResponse[]>> {
    return this.http.get<ApiResponse<GratuityRuleResponse[]>>(this.base);
  }

  getActive(): Observable<ApiResponse<GratuityRuleResponse>> {
    return this.http.get<ApiResponse<GratuityRuleResponse>>(`${this.base}/active`);
  }

  getById(id: number): Observable<ApiResponse<GratuityRuleResponse>> {
    return this.http.get<ApiResponse<GratuityRuleResponse>>(`${this.base}/${id}`);
  }

  create(dto: CreateGratuityRuleDto): Observable<ApiResponse<GratuityRuleResponse>> {
    return this.http.post<ApiResponse<GratuityRuleResponse>>(this.base, dto);
  }

  update(id: number, dto: UpdateGratuityRuleDto): Observable<ApiResponse<GratuityRuleResponse>> {
    return this.http.put<ApiResponse<GratuityRuleResponse>>(`${this.base}/${id}`, dto);
  }

  delete(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.base}/${id}`);
  }

  preview(dto: GratuityPreviewDto): Observable<ApiResponse<GratuityPreviewResult>> {
    return this.http.post<ApiResponse<GratuityPreviewResult>>(`${this.base}/preview`, dto);
  }
}

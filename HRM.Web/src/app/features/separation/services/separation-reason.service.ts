import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../core/models/api-response.model';
import {
  CreateSeparationReasonDto,
  SeparationReasonResponse,
  UpdateSeparationReasonDto,
} from '../models/separation-reason.model';

@Injectable({ providedIn: 'root' })
export class SeparationReasonService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/separation-reasons`;

  getAll(): Observable<ApiResponse<SeparationReasonResponse[]>> {
    return this.http.get<ApiResponse<SeparationReasonResponse[]>>(this.base);
  }

  getById(id: number): Observable<ApiResponse<SeparationReasonResponse>> {
    return this.http.get<ApiResponse<SeparationReasonResponse>>(`${this.base}/${id}`);
  }

  getByType(separationType: string): Observable<ApiResponse<SeparationReasonResponse[]>> {
    return this.http.get<ApiResponse<SeparationReasonResponse[]>>(`${this.base}/by-type/${separationType}`);
  }

  create(dto: CreateSeparationReasonDto): Observable<ApiResponse<SeparationReasonResponse>> {
    return this.http.post<ApiResponse<SeparationReasonResponse>>(this.base, dto);
  }

  update(id: number, dto: UpdateSeparationReasonDto): Observable<ApiResponse<SeparationReasonResponse>> {
    return this.http.put<ApiResponse<SeparationReasonResponse>>(`${this.base}/${id}`, dto);
  }

  delete(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.base}/${id}`);
  }
}

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
  CreateDesignationDto,
  DesignationFilter,
  DesignationResponse,
  UpdateDesignationDto,
} from '../models/designation.model';

@Injectable({ providedIn: 'root' })
export class DesignationService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/designations`;

  getAll(filter: DesignationFilter = {}): Observable<ApiResponse<PagedResult<DesignationResponse>>> {
    let params = new HttpParams();
    if (filter.departmentId) params = params.set('departmentId', String(filter.departmentId));
    if (filter.branchId)     params = params.set('branchId',     String(filter.branchId));
    if (typeof filter.isActive === 'boolean')
      params = params.set('isActive', String(filter.isActive));
    if (filter.pageNumber) params = params.set('pageNumber', String(filter.pageNumber));
    if (filter.pageSize)   params = params.set('pageSize',   String(filter.pageSize));
    return this.http
      .get<ApiResponse<DesignationResponse[] | PagedResult<DesignationResponse>>>(this.base, { params })
      .pipe(map((res) => toPagedResponse<DesignationResponse>(res)));
  }

  getById(id: number): Observable<ApiResponse<DesignationResponse>> {
    return this.http.get<ApiResponse<DesignationResponse>>(`${this.base}/${id}`);
  }

  create(dto: CreateDesignationDto): Observable<ApiResponse<DesignationResponse>> {
    return this.http.post<ApiResponse<DesignationResponse>>(this.base, dto);
  }

  update(id: number, dto: UpdateDesignationDto): Observable<ApiResponse<DesignationResponse>> {
    return this.http.put<ApiResponse<DesignationResponse>>(`${this.base}/${id}`, dto);
  }

  delete(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.base}/${id}`);
  }
}

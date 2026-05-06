import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import {
  ApiResponse,
  PagedResult,
} from '../../../core/models/api-response.model';
import {
  CreateDepartmentDto,
  DepartmentFilter,
  DepartmentResponse,
  UpdateDepartmentDto,
} from '../models/department.model';

@Injectable({ providedIn: 'root' })
export class DepartmentService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/departments`;

  getAll(filter: DepartmentFilter = {}): Observable<ApiResponse<PagedResult<DepartmentResponse>>> {
    let params = new HttpParams();
    if (filter.branchId) params = params.set('branchId', String(filter.branchId));
    if (typeof filter.isActive === 'boolean')
      params = params.set('isActive', String(filter.isActive));
    if (filter.pageNumber) params = params.set('pageNumber', String(filter.pageNumber));
    if (filter.pageSize)   params = params.set('pageSize',   String(filter.pageSize));
    return this.http.get<ApiResponse<PagedResult<DepartmentResponse>>>(this.base, { params });
  }

  getById(id: number): Observable<ApiResponse<DepartmentResponse>> {
    return this.http.get<ApiResponse<DepartmentResponse>>(`${this.base}/${id}`);
  }

  create(dto: CreateDepartmentDto): Observable<ApiResponse<DepartmentResponse>> {
    return this.http.post<ApiResponse<DepartmentResponse>>(this.base, dto);
  }

  update(id: number, dto: UpdateDepartmentDto): Observable<ApiResponse<DepartmentResponse>> {
    return this.http.put<ApiResponse<DepartmentResponse>>(`${this.base}/${id}`, dto);
  }

  delete(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.base}/${id}`);
  }
}

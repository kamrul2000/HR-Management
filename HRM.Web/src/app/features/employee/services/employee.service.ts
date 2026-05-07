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
  CreateEmployeeDto,
  EmployeeFilter,
  EmployeeResponse,
  UpdateEmployeeDto,
} from '../models/employee.model';

@Injectable({ providedIn: 'root' })
export class EmployeeService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/employees`;

  getAll(filter: EmployeeFilter = {}): Observable<ApiResponse<PagedResult<EmployeeResponse>>> {
    let params = new HttpParams();
    if (filter.search)         params = params.set('search', filter.search);
    if (filter.branchId)       params = params.set('branchId', String(filter.branchId));
    if (filter.departmentId)   params = params.set('departmentId', String(filter.departmentId));
    if (filter.designationId)  params = params.set('designationId', String(filter.designationId));
    if (filter.status)         params = params.set('status', filter.status);
    if (filter.employmentType) params = params.set('employmentType', filter.employmentType);
    if (typeof filter.isActive === 'boolean')
      params = params.set('isActive', String(filter.isActive));
    if (filter.pageNumber) params = params.set('pageNumber', String(filter.pageNumber));
    if (filter.pageSize)   params = params.set('pageSize',   String(filter.pageSize));
    return this.http
      .get<ApiResponse<EmployeeResponse[] | PagedResult<EmployeeResponse>>>(this.base, { params })
      .pipe(map((res) => toPagedResponse<EmployeeResponse>(res)));
  }

  getById(id: number): Observable<ApiResponse<EmployeeResponse>> {
    return this.http.get<ApiResponse<EmployeeResponse>>(`${this.base}/${id}`);
  }

  create(dto: CreateEmployeeDto): Observable<ApiResponse<EmployeeResponse>> {
    return this.http.post<ApiResponse<EmployeeResponse>>(this.base, dto);
  }

  update(id: number, dto: UpdateEmployeeDto): Observable<ApiResponse<EmployeeResponse>> {
    return this.http.put<ApiResponse<EmployeeResponse>>(`${this.base}/${id}`, dto);
  }

  delete(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.base}/${id}`);
  }

  uploadPhoto(id: number, file: File): Observable<ApiResponse<EmployeeResponse>> {
    const fd = new FormData();
    fd.append('file', file);
    return this.http.post<ApiResponse<EmployeeResponse>>(`${this.base}/${id}/photo`, fd);
  }
}

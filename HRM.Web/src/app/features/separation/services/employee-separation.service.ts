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
  ApproveSeparationDto,
  CancelSeparationDto,
  CreateSeparationDto,
  SeparationFilter,
  SeparationResponse,
} from '../models/employee-separation.model';

@Injectable({ providedIn: 'root' })
export class EmployeeSeparationService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/separations`;

  getAll(filter: SeparationFilter = {}): Observable<ApiResponse<PagedResult<SeparationResponse>>> {
    let params = new HttpParams();
    if (filter.separationType) params = params.set('separationType', filter.separationType);
    if (filter.status)         params = params.set('status', filter.status);
    if (filter.branchId)       params = params.set('branchId', String(filter.branchId));
    if (filter.fromDate)       params = params.set('fromDate', filter.fromDate);
    if (filter.toDate)         params = params.set('toDate', filter.toDate);
    if (filter.pageNumber)     params = params.set('pageNumber', String(filter.pageNumber));
    if (filter.pageSize)       params = params.set('pageSize', String(filter.pageSize));
    return this.http
      .get<ApiResponse<SeparationResponse[] | PagedResult<SeparationResponse>>>(this.base, { params })
      .pipe(map((res) => toPagedResponse<SeparationResponse>(res)));
  }

  getById(id: number): Observable<ApiResponse<SeparationResponse>> {
    return this.http.get<ApiResponse<SeparationResponse>>(`${this.base}/${id}`);
  }

  getByEmployee(employeeId: number): Observable<ApiResponse<SeparationResponse[]>> {
    return this.http.get<ApiResponse<SeparationResponse[]>>(`${this.base}/by-employee/${employeeId}`);
  }

  create(dto: CreateSeparationDto): Observable<ApiResponse<SeparationResponse>> {
    return this.http.post<ApiResponse<SeparationResponse>>(this.base, dto);
  }

  approve(id: number, dto: ApproveSeparationDto): Observable<ApiResponse<SeparationResponse>> {
    return this.http.put<ApiResponse<SeparationResponse>>(`${this.base}/${id}/approve`, dto);
  }

  process(id: number): Observable<ApiResponse<SeparationResponse>> {
    return this.http.put<ApiResponse<SeparationResponse>>(`${this.base}/${id}/process`, {});
  }

  cancel(id: number, dto: CancelSeparationDto): Observable<ApiResponse<SeparationResponse>> {
    return this.http.put<ApiResponse<SeparationResponse>>(`${this.base}/${id}/cancel`, dto);
  }

  uploadAttachment(id: number, file: File): Observable<ApiResponse<SeparationResponse>> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<ApiResponse<SeparationResponse>>(`${this.base}/${id}/attachment`, form);
  }
}

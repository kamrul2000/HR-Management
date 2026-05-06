import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import {
  ApiResponse,
  PagedResult,
} from '../../../core/models/api-response.model';
import {
  BranchFilter,
  BranchResponse,
  CreateBranchDto,
  UpdateBranchDto,
} from '../models/branch.model';

@Injectable({ providedIn: 'root' })
export class BranchService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/branches`;

  getAll(filter: BranchFilter = {}): Observable<ApiResponse<PagedResult<BranchResponse>>> {
    let params = new HttpParams();
    if (filter.companyId) params = params.set('companyId', String(filter.companyId));
    if (typeof filter.isActive === 'boolean')
      params = params.set('isActive', String(filter.isActive));
    if (filter.pageNumber) params = params.set('pageNumber', String(filter.pageNumber));
    if (filter.pageSize)   params = params.set('pageSize',   String(filter.pageSize));
    return this.http.get<ApiResponse<PagedResult<BranchResponse>>>(this.base, { params });
  }

  getById(id: number): Observable<ApiResponse<BranchResponse>> {
    return this.http.get<ApiResponse<BranchResponse>>(`${this.base}/${id}`);
  }

  create(dto: CreateBranchDto): Observable<ApiResponse<BranchResponse>> {
    return this.http.post<ApiResponse<BranchResponse>>(this.base, dto);
  }

  update(id: number, dto: UpdateBranchDto): Observable<ApiResponse<BranchResponse>> {
    return this.http.put<ApiResponse<BranchResponse>>(`${this.base}/${id}`, dto);
  }

  delete(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.base}/${id}`);
  }
}

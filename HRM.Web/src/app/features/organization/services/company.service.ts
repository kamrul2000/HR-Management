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
  CompanyResponse,
  CreateCompanyDto,
  UpdateCompanyDto,
} from '../models/company.model';

@Injectable({ providedIn: 'root' })
export class CompanyService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/companies`;

  getAll(opts: { pageNumber?: number; pageSize?: number; isActive?: boolean } = {}):
    Observable<ApiResponse<PagedResult<CompanyResponse>>> {
    let params = new HttpParams();
    if (opts.pageNumber) params = params.set('pageNumber', String(opts.pageNumber));
    if (opts.pageSize)   params = params.set('pageSize',   String(opts.pageSize));
    if (typeof opts.isActive === 'boolean')
      params = params.set('isActive', String(opts.isActive));
    return this.http
      .get<ApiResponse<CompanyResponse[] | PagedResult<CompanyResponse>>>(this.base, { params })
      .pipe(map((res) => toPagedResponse<CompanyResponse>(res)));
  }

  getById(id: number): Observable<ApiResponse<CompanyResponse>> {
    return this.http.get<ApiResponse<CompanyResponse>>(`${this.base}/${id}`);
  }

  create(dto: CreateCompanyDto): Observable<ApiResponse<CompanyResponse>> {
    return this.http.post<ApiResponse<CompanyResponse>>(this.base, dto);
  }

  update(id: number, dto: UpdateCompanyDto): Observable<ApiResponse<CompanyResponse>> {
    return this.http.put<ApiResponse<CompanyResponse>>(`${this.base}/${id}`, dto);
  }

  delete(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.base}/${id}`);
  }

  uploadLogo(id: number, file: File): Observable<ApiResponse<CompanyResponse>> {
    const fd = new FormData();
    fd.append('file', file);
    return this.http.post<ApiResponse<CompanyResponse>>(`${this.base}/${id}/logo`, fd);
  }
}

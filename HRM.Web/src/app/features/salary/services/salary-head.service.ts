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
  CreateSalaryHeadDto,
  SalaryHeadResponse,
  UpdateSalaryHeadDto,
} from '../models/salary-head.model';

@Injectable({ providedIn: 'root' })
export class SalaryHeadService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/salary-heads`;

  getAll(opts: { isActive?: boolean; pageSize?: number } = {}):
    Observable<ApiResponse<PagedResult<SalaryHeadResponse>>> {
    let params = new HttpParams();
    if (typeof opts.isActive === 'boolean') params = params.set('isActive', String(opts.isActive));
    if (opts.pageSize) params = params.set('pageSize', String(opts.pageSize));
    return this.http
      .get<ApiResponse<SalaryHeadResponse[] | PagedResult<SalaryHeadResponse>>>(this.base, { params })
      .pipe(map((res) => toPagedResponse<SalaryHeadResponse>(res)));
  }

  getById(id: number): Observable<ApiResponse<SalaryHeadResponse>> {
    return this.http.get<ApiResponse<SalaryHeadResponse>>(`${this.base}/${id}`);
  }

  create(dto: CreateSalaryHeadDto): Observable<ApiResponse<SalaryHeadResponse>> {
    return this.http.post<ApiResponse<SalaryHeadResponse>>(this.base, dto);
  }

  update(id: number, dto: UpdateSalaryHeadDto): Observable<ApiResponse<SalaryHeadResponse>> {
    return this.http.put<ApiResponse<SalaryHeadResponse>>(`${this.base}/${id}`, dto);
  }

  delete(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.base}/${id}`);
  }
}

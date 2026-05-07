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
  ApproveBonusDto,
  BonusFilter,
  BonusResponse,
  CreateBonusDto,
} from '../models/bonus.model';

@Injectable({ providedIn: 'root' })
export class BonusService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/bonus`;

  getAll(filter: BonusFilter = {}): Observable<ApiResponse<PagedResult<BonusResponse>>> {
    let params = new HttpParams();
    if (filter.employeeId) params = params.set('employeeId', String(filter.employeeId));
    if (filter.branchId)   params = params.set('branchId',   String(filter.branchId));
    if (filter.status)     params = params.set('status', filter.status);
    if (filter.year)       params = params.set('year', String(filter.year));
    if (filter.month)      params = params.set('month', String(filter.month));
    if (filter.pageNumber) params = params.set('pageNumber', String(filter.pageNumber));
    if (filter.pageSize)   params = params.set('pageSize', String(filter.pageSize));
    return this.http
      .get<ApiResponse<BonusResponse[] | PagedResult<BonusResponse>>>(this.base, { params })
      .pipe(map((res) => toPagedResponse<BonusResponse>(res)));
  }

  getById(id: number): Observable<ApiResponse<BonusResponse>> {
    return this.http.get<ApiResponse<BonusResponse>>(`${this.base}/${id}`);
  }

  create(dto: CreateBonusDto): Observable<ApiResponse<BonusResponse>> {
    return this.http.post<ApiResponse<BonusResponse>>(this.base, dto);
  }

  approve(id: number, dto: ApproveBonusDto): Observable<ApiResponse<BonusResponse>> {
    return this.http.put<ApiResponse<BonusResponse>>(`${this.base}/${id}/approve`, dto);
  }

  reject(id: number, remarks: string): Observable<ApiResponse<BonusResponse>> {
    return this.http.put<ApiResponse<BonusResponse>>(`${this.base}/${id}/reject`, { approvalRemarks: remarks });
  }

  cancel(id: number): Observable<ApiResponse<BonusResponse>> {
    return this.http.put<ApiResponse<BonusResponse>>(`${this.base}/${id}/cancel`, {});
  }

  delete(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.base}/${id}`);
  }
}

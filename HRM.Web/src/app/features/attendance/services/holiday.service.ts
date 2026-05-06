import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import {
  ApiResponse,
  PagedResult,
} from '../../../core/models/api-response.model';
import {
  CreateHolidayDto,
  HolidayFilter,
  HolidayResponse,
  UpdateHolidayDto,
} from '../models/holiday.model';

@Injectable({ providedIn: 'root' })
export class HolidayService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/holidays`;

  getAll(filter: HolidayFilter = {}): Observable<ApiResponse<PagedResult<HolidayResponse>>> {
    let params = new HttpParams();
    if (filter.year)        params = params.set('year', String(filter.year));
    if (filter.month)       params = params.set('month', String(filter.month));
    if (filter.branchId)    params = params.set('branchId', String(filter.branchId));
    if (filter.holidayType) params = params.set('holidayType', filter.holidayType);
    if (typeof filter.isActive === 'boolean') {
      params = params.set('isActive', String(filter.isActive));
    }
    params = params.set('pageSize', '300');
    return this.http.get<ApiResponse<PagedResult<HolidayResponse>>>(this.base, { params });
  }

  getById(id: number): Observable<ApiResponse<HolidayResponse>> {
    return this.http.get<ApiResponse<HolidayResponse>>(`${this.base}/${id}`);
  }

  create(dto: CreateHolidayDto): Observable<ApiResponse<HolidayResponse>> {
    return this.http.post<ApiResponse<HolidayResponse>>(this.base, dto);
  }

  update(id: number, dto: UpdateHolidayDto): Observable<ApiResponse<HolidayResponse>> {
    return this.http.put<ApiResponse<HolidayResponse>>(`${this.base}/${id}`, dto);
  }

  delete(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.base}/${id}`);
  }
}

import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import {
  ApiResponse,
  PagedResult,
} from '../../../core/models/api-response.model';
import {
  CreateDutySlotDto,
  DutySlotResponse,
  UpdateDutySlotDto,
} from '../models/duty-slot.model';

@Injectable({ providedIn: 'root' })
export class DutySlotService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/duty-slots`;

  getAll(opts: { isActive?: boolean; pageNumber?: number; pageSize?: number } = {}):
    Observable<ApiResponse<PagedResult<DutySlotResponse>>> {
    let params = new HttpParams();
    if (typeof opts.isActive === 'boolean') params = params.set('isActive', String(opts.isActive));
    if (opts.pageNumber) params = params.set('pageNumber', String(opts.pageNumber));
    if (opts.pageSize)   params = params.set('pageSize',   String(opts.pageSize));
    return this.http.get<ApiResponse<PagedResult<DutySlotResponse>>>(this.base, { params });
  }

  getById(id: number): Observable<ApiResponse<DutySlotResponse>> {
    return this.http.get<ApiResponse<DutySlotResponse>>(`${this.base}/${id}`);
  }

  create(dto: CreateDutySlotDto): Observable<ApiResponse<DutySlotResponse>> {
    return this.http.post<ApiResponse<DutySlotResponse>>(this.base, dto);
  }

  update(id: number, dto: UpdateDutySlotDto): Observable<ApiResponse<DutySlotResponse>> {
    return this.http.put<ApiResponse<DutySlotResponse>>(`${this.base}/${id}`, dto);
  }

  delete(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.base}/${id}`);
  }
}

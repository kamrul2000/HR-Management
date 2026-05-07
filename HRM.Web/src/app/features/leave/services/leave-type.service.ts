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
  CreateLeaveTypeDto,
  LeaveTypeResponse,
  UpdateLeaveTypeDto,
} from '../models/leave-type.model';

@Injectable({ providedIn: 'root' })
export class LeaveTypeService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/leave-types`;

  getAll(opts: { isActive?: boolean; pageSize?: number } = {}):
    Observable<ApiResponse<PagedResult<LeaveTypeResponse>>> {
    let params = new HttpParams();
    if (typeof opts.isActive === 'boolean') params = params.set('isActive', String(opts.isActive));
    if (opts.pageSize) params = params.set('pageSize', String(opts.pageSize));
    return this.http
      .get<ApiResponse<LeaveTypeResponse[] | PagedResult<LeaveTypeResponse>>>(this.base, { params })
      .pipe(map((res) => toPagedResponse<LeaveTypeResponse>(res)));
  }

  getById(id: number): Observable<ApiResponse<LeaveTypeResponse>> {
    return this.http.get<ApiResponse<LeaveTypeResponse>>(`${this.base}/${id}`);
  }

  create(dto: CreateLeaveTypeDto): Observable<ApiResponse<LeaveTypeResponse>> {
    return this.http.post<ApiResponse<LeaveTypeResponse>>(this.base, dto);
  }

  update(id: number, dto: UpdateLeaveTypeDto): Observable<ApiResponse<LeaveTypeResponse>> {
    return this.http.put<ApiResponse<LeaveTypeResponse>>(`${this.base}/${id}`, dto);
  }

  delete(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.base}/${id}`);
  }
}

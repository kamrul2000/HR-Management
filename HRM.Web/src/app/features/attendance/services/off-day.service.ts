import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../core/models/api-response.model';
import {
  BulkSetOffDaysDto,
  CreateOffDayDto,
  OffDayResponse,
} from '../models/off-day.model';

@Injectable({ providedIn: 'root' })
export class OffDayService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/off-days`;

  getAll(opts: { branchId?: number | null } = {}): Observable<ApiResponse<OffDayResponse[]>> {
    let params = new HttpParams();
    if (typeof opts.branchId === 'number') {
      params = params.set('branchId', String(opts.branchId));
    } else if (opts.branchId === null) {
      params = params.set('organizationWide', 'true');
    }
    return this.http.get<ApiResponse<OffDayResponse[]>>(this.base, { params });
  }

  create(dto: CreateOffDayDto): Observable<ApiResponse<OffDayResponse>> {
    return this.http.post<ApiResponse<OffDayResponse>>(this.base, dto);
  }

  delete(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.base}/${id}`);
  }

  /** Replace the off-day set for a scope (org-wide if branchId is null). */
  bulkSet(dto: BulkSetOffDaysDto): Observable<ApiResponse<OffDayResponse[]>> {
    return this.http.post<ApiResponse<OffDayResponse[]>>(`${this.base}/bulk-set`, dto);
  }
}

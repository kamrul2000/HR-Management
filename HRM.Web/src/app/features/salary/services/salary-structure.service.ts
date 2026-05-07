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
  CreateSalaryStructureDto,
  SalaryStructureFilter,
  SalaryStructureHistoryDto,
  SalaryStructureResponse,
  UpdateSalaryStructureDto,
} from '../models/salary-structure.model';

@Injectable({ providedIn: 'root' })
export class SalaryStructureService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/salary-structures`;

  getAll(filter: SalaryStructureFilter = {}):
    Observable<ApiResponse<PagedResult<SalaryStructureResponse>>> {
    let params = new HttpParams();
    if (filter.employeeId) params = params.set('employeeId', String(filter.employeeId));
    if (filter.branchId)   params = params.set('branchId',   String(filter.branchId));
    if (typeof filter.isActive === 'boolean')
      params = params.set('isActive', String(filter.isActive));
    if (filter.pageNumber) params = params.set('pageNumber', String(filter.pageNumber));
    if (filter.pageSize)   params = params.set('pageSize',   String(filter.pageSize));
    return this.http
      .get<ApiResponse<SalaryStructureResponse[] | PagedResult<SalaryStructureResponse>>>(
        this.base, { params })
      .pipe(map((res) => toPagedResponse<SalaryStructureResponse>(res)));
  }

  getById(id: number): Observable<ApiResponse<SalaryStructureResponse>> {
    return this.http.get<ApiResponse<SalaryStructureResponse>>(`${this.base}/${id}`);
  }

  getActiveByEmployee(employeeId: number): Observable<ApiResponse<SalaryStructureResponse>> {
    return this.http.get<ApiResponse<SalaryStructureResponse>>(`${this.base}/active/${employeeId}`);
  }

  getHistoryByEmployee(employeeId: number):
    Observable<ApiResponse<SalaryStructureHistoryDto[]>> {
    return this.http.get<ApiResponse<SalaryStructureHistoryDto[]>>(
      `${this.base}/history/${employeeId}`,
    );
  }

  create(dto: CreateSalaryStructureDto): Observable<ApiResponse<SalaryStructureResponse>> {
    return this.http.post<ApiResponse<SalaryStructureResponse>>(this.base, dto);
  }

  update(id: number, dto: UpdateSalaryStructureDto): Observable<ApiResponse<SalaryStructureResponse>> {
    return this.http.put<ApiResponse<SalaryStructureResponse>>(`${this.base}/${id}`, dto);
  }

  deactivate(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.base}/${id}`);
  }
}

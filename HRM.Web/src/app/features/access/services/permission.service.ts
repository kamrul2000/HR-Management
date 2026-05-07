import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../core/models/api-response.model';
import {
  BulkUpsertPermissionsDto,
  PermissionResponse,
  UpsertPermissionDto,
  UserPermissionSummary,
} from '../models/permission.model';

@Injectable({ providedIn: 'root' })
export class PermissionService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/permissions`;

  getAll(): Observable<ApiResponse<PermissionResponse[]>> {
    return this.http.get<ApiResponse<PermissionResponse[]>>(this.base);
  }

  getByRole(roleId: number): Observable<ApiResponse<PermissionResponse[]>> {
    return this.http.get<ApiResponse<PermissionResponse[]>>(`${this.base}/by-role/${roleId}`);
  }

  getMyPermissions(): Observable<ApiResponse<UserPermissionSummary>> {
    return this.http.get<ApiResponse<UserPermissionSummary>>(`${this.base}/my-permissions`);
  }

  upsert(dto: UpsertPermissionDto): Observable<ApiResponse<PermissionResponse>> {
    return this.http.post<ApiResponse<PermissionResponse>>(this.base, dto);
  }

  bulkUpsert(dto: BulkUpsertPermissionsDto): Observable<ApiResponse<PermissionResponse[]>> {
    return this.http.post<ApiResponse<PermissionResponse[]>>(`${this.base}/bulk`, dto);
  }

  delete(id: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.base}/${id}`);
  }
}

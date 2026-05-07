import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../core/models/api-response.model';
import { AssignRoleDto, UserRoleResponse } from '../models/user-role.model';

@Injectable({ providedIn: 'root' })
export class UserRoleService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/user-roles`;

  getAllActive(): Observable<ApiResponse<UserRoleResponse[]>> {
    return this.http.get<ApiResponse<UserRoleResponse[]>>(`${this.base}/active`);
  }

  getByUser(userId: number): Observable<ApiResponse<UserRoleResponse[]>> {
    return this.http.get<ApiResponse<UserRoleResponse[]>>(`${this.base}/by-user/${userId}`);
  }

  getByRole(roleId: number): Observable<ApiResponse<UserRoleResponse[]>> {
    return this.http.get<ApiResponse<UserRoleResponse[]>>(`${this.base}/by-role/${roleId}`);
  }

  assign(dto: AssignRoleDto): Observable<ApiResponse<UserRoleResponse>> {
    return this.http.post<ApiResponse<UserRoleResponse>>(this.base, dto);
  }

  revoke(id: number): Observable<ApiResponse<UserRoleResponse>> {
    return this.http.put<ApiResponse<UserRoleResponse>>(`${this.base}/${id}/revoke`, {});
  }
}

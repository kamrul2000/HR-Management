import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../core/models/api-response.model';
import {
  CreateRecommendationDto,
  LoanRecommendationResponse,
} from '../models/loan-recommendation.model';

@Injectable({ providedIn: 'root' })
export class LoanRecommendationService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/loan-recommendations`;

  getAll(): Observable<ApiResponse<LoanRecommendationResponse[]>> {
    return this.http.get<ApiResponse<LoanRecommendationResponse[]>>(this.base);
  }

  getById(id: number): Observable<ApiResponse<LoanRecommendationResponse>> {
    return this.http.get<ApiResponse<LoanRecommendationResponse>>(`${this.base}/${id}`);
  }

  getByApplication(loanApplicationId: number): Observable<ApiResponse<LoanRecommendationResponse>> {
    return this.http.get<ApiResponse<LoanRecommendationResponse>>(`${this.base}/by-application/${loanApplicationId}`);
  }

  recommend(dto: CreateRecommendationDto): Observable<ApiResponse<LoanRecommendationResponse>> {
    return this.http.post<ApiResponse<LoanRecommendationResponse>>(this.base, dto);
  }
}

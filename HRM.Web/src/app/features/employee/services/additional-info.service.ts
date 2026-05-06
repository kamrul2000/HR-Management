import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../core/models/api-response.model';
import {
  CreateEducationDto,
  CreateEmergencyContactDto,
  CreateExperienceDto,
  EducationDto,
  EmergencyContactDto,
  ExperienceDto,
  UpdateEducationDto,
  UpdateEmergencyContactDto,
  UpdateExperienceDto,
} from '../models/additional-info.model';

@Injectable({ providedIn: 'root' })
export class AdditionalInfoService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/additional-info`;

  // ───────────────────── Emergency Contacts
  getContacts(employeeId: number): Observable<ApiResponse<EmergencyContactDto[]>> {
    return this.http.get<ApiResponse<EmergencyContactDto[]>>(
      `${this.base}/${employeeId}/emergency-contacts`,
    );
  }

  addContact(employeeId: number, dto: CreateEmergencyContactDto): Observable<ApiResponse<EmergencyContactDto>> {
    return this.http.post<ApiResponse<EmergencyContactDto>>(
      `${this.base}/${employeeId}/emergency-contacts`,
      dto,
    );
  }

  updateContact(contactId: number, dto: UpdateEmergencyContactDto): Observable<ApiResponse<EmergencyContactDto>> {
    return this.http.put<ApiResponse<EmergencyContactDto>>(
      `${this.base}/emergency-contacts/${contactId}`,
      dto,
    );
  }

  setPrimaryContact(contactId: number): Observable<ApiResponse<EmergencyContactDto>> {
    return this.http.put<ApiResponse<EmergencyContactDto>>(
      `${this.base}/emergency-contacts/${contactId}/set-primary`,
      {},
    );
  }

  deleteContact(contactId: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(
      `${this.base}/emergency-contacts/${contactId}`,
    );
  }

  // ───────────────────── Education
  getEducation(employeeId: number): Observable<ApiResponse<EducationDto[]>> {
    return this.http.get<ApiResponse<EducationDto[]>>(
      `${this.base}/${employeeId}/education`,
    );
  }

  addEducation(employeeId: number, dto: CreateEducationDto): Observable<ApiResponse<EducationDto>> {
    return this.http.post<ApiResponse<EducationDto>>(
      `${this.base}/${employeeId}/education`,
      dto,
    );
  }

  updateEducation(educationId: number, dto: UpdateEducationDto): Observable<ApiResponse<EducationDto>> {
    return this.http.put<ApiResponse<EducationDto>>(
      `${this.base}/education/${educationId}`,
      dto,
    );
  }

  uploadEducationAttachment(educationId: number, file: File): Observable<ApiResponse<EducationDto>> {
    const fd = new FormData();
    fd.append('file', file);
    return this.http.post<ApiResponse<EducationDto>>(
      `${this.base}/education/${educationId}/attachment`,
      fd,
    );
  }

  deleteEducation(educationId: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.base}/education/${educationId}`);
  }

  // ───────────────────── Experience
  getExperience(employeeId: number): Observable<ApiResponse<ExperienceDto[]>> {
    return this.http.get<ApiResponse<ExperienceDto[]>>(
      `${this.base}/${employeeId}/experience`,
    );
  }

  addExperience(employeeId: number, dto: CreateExperienceDto): Observable<ApiResponse<ExperienceDto>> {
    return this.http.post<ApiResponse<ExperienceDto>>(
      `${this.base}/${employeeId}/experience`,
      dto,
    );
  }

  updateExperience(experienceId: number, dto: UpdateExperienceDto): Observable<ApiResponse<ExperienceDto>> {
    return this.http.put<ApiResponse<ExperienceDto>>(
      `${this.base}/experience/${experienceId}`,
      dto,
    );
  }

  uploadExperienceAttachment(experienceId: number, file: File): Observable<ApiResponse<ExperienceDto>> {
    const fd = new FormData();
    fd.append('file', file);
    return this.http.post<ApiResponse<ExperienceDto>>(
      `${this.base}/experience/${experienceId}/attachment`,
      fd,
    );
  }

  deleteExperience(experienceId: number): Observable<ApiResponse<unknown>> {
    return this.http.delete<ApiResponse<unknown>>(`${this.base}/experience/${experienceId}`);
  }
}

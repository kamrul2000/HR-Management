export interface EmergencyContactDto {
  id: number;
  employeeId: number;
  contactName: string;
  relationship: string;
  phone: string;
  alternatePhone?: string | null;
  address?: string | null;
  isPrimary: boolean;
  subscriptionId: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateEmergencyContactDto {
  contactName: string;
  relationship: string;
  phone: string;
  alternatePhone?: string | null;
  address?: string | null;
  isPrimary: boolean;
}

export interface UpdateEmergencyContactDto extends CreateEmergencyContactDto {}

export interface EducationDto {
  id: number;
  employeeId: number;
  degree: string;
  institution: string;
  passingYear: number;
  result?: string | null;
  majorSubject?: string | null;
  attachmentPath?: string | null;
  attachmentUrl?: string | null;
  subscriptionId: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateEducationDto {
  degree: string;
  institution: string;
  passingYear: number;
  result?: string | null;
  majorSubject?: string | null;
}

export interface UpdateEducationDto extends CreateEducationDto {}

export interface ExperienceDto {
  id: number;
  employeeId: number;
  organizationName: string;
  designation: string;
  fromDate: string;
  fromDateFormatted?: string;
  toDate?: string | null;
  toDateFormatted?: string | null;
  isCurrent: boolean;
  responsibilities?: string | null;
  reasonForLeaving?: string | null;
  attachmentPath?: string | null;
  attachmentUrl?: string | null;
  serviceDurationLabel?: string;
  subscriptionId: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateExperienceDto {
  organizationName: string;
  designation: string;
  fromDate: string;
  toDate?: string | null;
  isCurrent: boolean;
  responsibilities?: string | null;
  reasonForLeaving?: string | null;
}

export interface UpdateExperienceDto extends CreateExperienceDto {}

export type Gender = 'Male' | 'Female' | 'Other';
export type MaritalStatus = 'Single' | 'Married' | 'Divorced' | 'Widowed';
export type EmploymentType = 'Permanent' | 'Contract' | 'Probationary' | 'Internship';
export type EmployeeStatus =
  | 'Active'
  | 'Resigned'
  | 'Terminated'
  | 'Retired'
  | 'Inactive';

export interface EmployeeResponse {
  id: number;
  employeeCode: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  phone: string;
  dateOfBirth: string;
  dateOfBirthFormatted?: string;
  gender: Gender;
  maritalStatus: MaritalStatus;
  nationalId?: string | null;
  joiningDate: string;
  joiningDateFormatted?: string;
  confirmationDate?: string | null;
  confirmationDateFormatted?: string | null;
  address: string;
  photoPath?: string | null;
  photoUrl?: string | null;
  branchId: number;
  branchName?: string;
  departmentId: number;
  departmentName?: string;
  designationId: number;
  designationTitle?: string;
  employmentType: EmploymentType;
  status: EmployeeStatus;
  statusLabel?: string;
  isActive: boolean;
  subscriptionId: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateEmployeeDto {
  employeeCode: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  dateOfBirth: string;
  gender: Gender;
  maritalStatus: MaritalStatus;
  nationalId?: string | null;
  joiningDate: string;
  confirmationDate?: string | null;
  address: string;
  branchId: number;
  departmentId: number;
  designationId: number;
  employmentType: EmploymentType;
}

export interface UpdateEmployeeDto extends CreateEmployeeDto {
  status: EmployeeStatus;
}

export interface EmployeeFilter {
  search?: string;
  branchId?: number;
  departmentId?: number;
  designationId?: number;
  status?: EmployeeStatus | string;
  employmentType?: EmploymentType | string;
  isActive?: boolean;
  pageNumber?: number;
  pageSize?: number;
}

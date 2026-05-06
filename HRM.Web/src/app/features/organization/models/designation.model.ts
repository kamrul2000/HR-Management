export interface DesignationResponse {
  id: number;
  title: string;
  description?: string | null;
  grade?: string | null;
  departmentId: number;
  departmentName?: string;
  branchId?: number;
  branchName?: string;
  companyId?: number;
  companyName?: string;
  isActive: boolean;
  subscriptionId: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateDesignationDto {
  title: string;
  description?: string | null;
  grade?: string | null;
  departmentId: number;
}

export interface UpdateDesignationDto {
  title: string;
  description?: string | null;
  grade?: string | null;
  departmentId: number;
  isActive: boolean;
}

export interface DesignationFilter {
  departmentId?: number;
  branchId?: number;
  isActive?: boolean;
  pageNumber?: number;
  pageSize?: number;
}

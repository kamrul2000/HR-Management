export interface DepartmentResponse {
  id: number;
  name: string;
  description?: string | null;
  branchId: number;
  branchName?: string;
  companyId?: number;
  companyName?: string;
  isActive: boolean;
  subscriptionId: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateDepartmentDto {
  name: string;
  description?: string | null;
  branchId: number;
}

export interface UpdateDepartmentDto {
  name: string;
  description?: string | null;
  branchId: number;
  isActive: boolean;
}

export interface DepartmentFilter {
  branchId?: number;
  isActive?: boolean;
  pageNumber?: number;
  pageSize?: number;
}

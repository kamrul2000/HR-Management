export interface BranchResponse {
  id: number;
  name: string;
  code: string;
  address: string;
  phone: string;
  email: string;
  managerName?: string | null;
  companyId: number;
  companyName?: string;
  isActive: boolean;
  subscriptionId: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateBranchDto {
  name: string;
  code: string;
  address: string;
  phone: string;
  email: string;
  managerName?: string | null;
  companyId: number;
}

export interface UpdateBranchDto {
  name: string;
  code: string;
  address: string;
  phone: string;
  email: string;
  managerName?: string | null;
  companyId: number;
  isActive: boolean;
}

export interface BranchFilter {
  companyId?: number;
  isActive?: boolean;
  pageNumber?: number;
  pageSize?: number;
}

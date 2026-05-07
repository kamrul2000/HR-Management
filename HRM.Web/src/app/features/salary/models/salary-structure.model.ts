export interface SalaryStructureItemDto {
  id?: number;
  salaryHeadId: number;
  headName?: string;
  headCode?: string;
  headType?: string;
  headTypeLabel?: string;
  calculationMethod?: string;
  fixedAmount?: number | null;
  overridePercentage?: number | null;
  effectivePercentage?: number | null;
  isTaxable?: boolean;
  isProvidentFundApplicable?: boolean;
  displayOrder?: number;
}

export interface SalaryStructureResponse {
  id: number;
  employeeId: number;
  employeeFullName?: string;
  employeeCode?: string;
  effectiveFrom: string;
  effectiveFromFormatted?: string;
  effectiveTo?: string | null;
  effectiveToFormatted?: string | null;
  basicSalary: number;
  basicSalaryFormatted?: string;
  estimatedGrossSalary: number;
  estimatedDeductions: number;
  estimatedNetSalary: number;
  isActive: boolean;
  remarks?: string | null;
  items: SalaryStructureItemDto[];
  subscriptionId: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateSalaryStructureDto {
  employeeId: number;
  effectiveFrom: string;
  basicSalary: number;
  remarks?: string | null;
  items: SalaryStructureItemDto[];
}

export interface UpdateSalaryStructureDto {
  remarks?: string | null;
  items: SalaryStructureItemDto[];
}

export interface SalaryStructureFilter {
  employeeId?: number;
  isActive?: boolean;
  branchId?: number;
  pageNumber?: number;
  pageSize?: number;
}

export interface SalaryStructureHistoryDto {
  id: number;
  effectiveFrom: string;
  effectiveFromFormatted?: string;
  effectiveTo?: string | null;
  effectiveToFormatted?: string | null;
  basicSalary: number;
  estimatedGrossSalary: number;
  isActive: boolean;
  remarks?: string | null;
  createdAt: string;
}

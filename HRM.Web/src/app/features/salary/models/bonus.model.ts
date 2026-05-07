export type BonusStatus = 'Pending' | 'Approved' | 'Rejected' | 'Disbursed' | 'Cancelled';
export type BonusBasis = 'Fixed' | 'PercentageOfBasic' | 'PercentageOfGross';

export interface BonusResponse {
  id: number;
  employeeId: number;
  employeeFullName?: string;
  employeeCode?: string;
  bonusType: string;
  bonusTitle: string;
  calculationBasis: BonusBasis;
  basisPercentage?: number | null;
  basicSalarySnapshot: number;
  grossSalarySnapshot: number;
  computedAmount: number;
  computedAmountFormatted?: string;
  finalAmount: number;
  finalAmountFormatted?: string;
  disbursementMonth: number;
  disbursementYear: number;
  isDisbursedWithSalary: boolean;
  status: BonusStatus;
  statusLabel?: string;
  approvalRemarks?: string | null;
  remarks?: string | null;
  salaryCalculationId?: number | null;
  subscriptionId: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateBonusDto {
  employeeId: number;
  bonusType: string;
  bonusTitle: string;
  calculationBasis: BonusBasis;
  basisPercentage?: number | null;
  fixedAmount?: number | null;
  disbursementMonth: number;
  disbursementYear: number;
  isDisbursedWithSalary: boolean;
  remarks?: string | null;
}

export interface ApproveBonusDto {
  finalAmount?: number | null;
  approvalRemarks?: string | null;
}

export interface BonusFilter {
  employeeId?: number;
  branchId?: number;
  status?: BonusStatus | string;
  year?: number;
  month?: number;
  pageNumber?: number;
  pageSize?: number;
}

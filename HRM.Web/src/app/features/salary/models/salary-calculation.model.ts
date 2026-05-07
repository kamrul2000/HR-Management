export type SalaryCalculationStatus = 'Draft' | 'Finalized' | 'Cancelled';

export interface SalaryCalculationDetailResponseDto {
  id: number;
  salaryHeadId: number;
  headName: string;
  headCode: string;
  headType: string;
  headTypeLabel: string;
  calculationMethod: string;
  baseAmount?: number | null;
  appliedPercentage?: number | null;
  computedAmount: number;
  computedAmountFormatted?: string;
  displayOrder: number;
}

export interface SalaryCalculationResponse {
  id: number;
  employeeId: number;
  employeeFullName?: string;
  employeeCode?: string;
  branchName?: string;
  designationTitle?: string;
  salaryStructureId: number;
  year: number;
  month: number;
  monthLabel?: string;
  totalWorkingDays: number;
  presentDays: number;
  absentDays: number;
  halfDays: number;
  unpaidLeaveDays: number;
  lateDeductionDays: number;
  overtimeMinutes: number;
  overtimeFormatted?: string;
  basicSalary: number;
  basicSalaryFormatted?: string;
  grossSalary: number;
  grossSalaryFormatted?: string;
  totalEarnings: number;
  totalEarningsFormatted?: string;
  attendanceDeduction: number;
  attendanceDeductionFormatted?: string;
  totalDeductions: number;
  totalDeductionsFormatted?: string;
  overtimePay: number;
  overtimePayFormatted?: string;
  bonusAmount: number;
  loanDeduction: number;
  taxDeduction: number;
  netSalary: number;
  netSalaryFormatted?: string;
  status: SalaryCalculationStatus;
  statusLabel?: string;
  remarks?: string | null;
  earningDetails: SalaryCalculationDetailResponseDto[];
  deductionDetails: SalaryCalculationDetailResponseDto[];
  finalizedAt?: string | null;
  cancelledAt?: string | null;
  subscriptionId: number;
  createdAt: string;
  updatedAt: string;
}

export interface RunSalaryCalculationDto {
  employeeId: number;
  year: number;
  month: number;
  remarks?: string | null;
}

export interface BulkRunSalaryDto {
  year: number;
  month: number;
  branchId?: number;
  employeeIds?: number[];
  remarks?: string | null;
}

export interface SalaryCalculationFilter {
  employeeId?: number;
  branchId?: number;
  departmentId?: number;
  year?: number;
  month?: number;
  status?: SalaryCalculationStatus | string;
  pageNumber?: number;
  pageSize?: number;
}

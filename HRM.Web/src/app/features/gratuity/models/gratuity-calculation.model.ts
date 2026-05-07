export type GratuityStatus = 'Draft' | 'Finalized' | 'Cancelled' | string;

export interface GratuityCalculationResponse {
  id: number;
  employeeId: number;
  employeeCode: string;
  employeeFullName: string;
  gratuityRuleId: number;
  ruleName: string;
  separationDate: string;
  separationDateFormatted?: string;
  joiningDate: string;
  joiningDateFormatted?: string;
  totalServiceDays: number;
  totalServiceYears: number;
  servicePeriodLabel?: string;
  eligibleYears: number;
  calculationBasis: string;
  calculationBasisLabel?: string;
  monthlySalaryUsed: number;
  monthlySalaryFormatted?: string;
  dailySalary: number;
  dailySalaryFormatted?: string;
  ratePerYear: number;
  gratuityBeforeCap: number;
  gratuityBeforeCapFormatted?: string;
  gratuityAmount: number;
  gratuityAmountFormatted?: string;
  isCapApplied: boolean;
  isEligible: boolean;
  ineligibilityReason?: string | null;
  separationId?: number | null;
  status: GratuityStatus;
  statusLabel?: string;
  remarks?: string | null;
  subscriptionId: number;
  createdAt: string;
  updatedAt: string;
}

export interface ComputeGratuityDto {
  employeeId: number;
  separationDate: string;
  gratuityRuleId?: number | null;
  remarks?: string | null;
}

export interface GratuityReport {
  branchId?: number | null;
  branchName?: string | null;
  totalRecords: number;
  eligibleCount: number;
  ineligibleCount: number;
  totalGratuityAmount: number;
  totalGratuityAmountFormatted?: string;
  details: GratuityCalculationResponse[];
}

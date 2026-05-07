export type GratuityCalculationBasis = 'Basic' | 'Gross' | 'LastDrawn' | string;

export interface GratuityRuleResponse {
  id: number;
  ruleName: string;
  minServiceYears: number;
  minServiceYearsLabel?: string;
  calculationBasis: GratuityCalculationBasis;
  calculationBasisLabel?: string;
  ratePerYear: number;
  ratePerYearLabel?: string;
  maxGratuityAmount?: number | null;
  maxGratuityAmountFormatted?: string | null;
  maxServiceYearsCapped?: number | null;
  maxServiceYearsCappedLabel?: string | null;
  proRataEnabled: boolean;
  proRataLabel?: string;
  isActive: boolean;
  effectiveFrom: string;
  effectiveFromFormatted?: string;
  description?: string | null;
  subscriptionId: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateGratuityRuleDto {
  ruleName: string;
  minServiceYears: number;
  calculationBasis: GratuityCalculationBasis;
  ratePerYear: number;
  maxGratuityAmount?: number | null;
  maxServiceYearsCapped?: number | null;
  proRataEnabled: boolean;
  effectiveFrom: string;
  description?: string | null;
}

export interface UpdateGratuityRuleDto {
  ruleName: string;
  minServiceYears: number;
  calculationBasis: GratuityCalculationBasis;
  ratePerYear: number;
  maxGratuityAmount?: number | null;
  maxServiceYearsCapped?: number | null;
  proRataEnabled: boolean;
  isActive: boolean;
  description?: string | null;
}

export interface GratuityPreviewDto {
  employeeId: number;
  separationDate: string;
  gratuityRuleId?: number | null;
}

export interface GratuityPreviewResult {
  employeeId: number;
  employeeFullName: string;
  employeeCode: string;
  joiningDate: string;
  joiningDateFormatted?: string;
  separationDate: string;
  separationDateFormatted?: string;
  totalServiceYears: number;
  servicePeriodLabel?: string;
  isEligible: boolean;
  ineligibilityReason: string;
  monthlySalaryUsed: number;
  monthlySalaryFormatted?: string;
  dailySalary: number;
  eligibleYears: number;
  ratePerYear: number;
  gratuityBeforeCap: number;
  gratuityAmount: number;
  gratuityAmountFormatted?: string;
  isCapApplied: boolean;
  ruleName: string;
  calculationBasis: string;
}

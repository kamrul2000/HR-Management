export type PfBase = 'Basic' | 'Gross' | string;

export interface PfRuleResponse {
  id: number;
  ruleName: string;
  employeeContributionRate: number;
  employeeContributionRateLabel?: string;
  employerContributionRate: number;
  employerContributionRateLabel?: string;
  pfBase: PfBase;
  pfBaseLabel?: string;
  minEligibleSalary?: number | null;
  minEligibleSalaryFormatted?: string | null;
  maxContributionAmount?: number | null;
  maxContributionAmountFormatted?: string | null;
  effectiveFrom: string;
  effectiveFromFormatted?: string;
  effectiveTo?: string | null;
  effectiveToFormatted?: string | null;
  isActive: boolean;
  description?: string | null;
  subscriptionId: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreatePfRuleDto {
  ruleName: string;
  employeeContributionRate: number;
  employerContributionRate: number;
  pfBase: PfBase;
  minEligibleSalary?: number | null;
  maxContributionAmount?: number | null;
  effectiveFrom: string;
  description?: string | null;
}

export interface UpdatePfRuleDto {
  ruleName: string;
  employeeContributionRate: number;
  employerContributionRate: number;
  minEligibleSalary?: number | null;
  maxContributionAmount?: number | null;
  effectiveTo?: string | null;
  isActive: boolean;
  description?: string | null;
}

export interface EmployeePfContributionResponse {
  id: number;
  employeeId: number;
  employeeCode: string;
  employeeFullName: string;
  pfRuleId: number;
  ruleName: string;
  year: number;
  month: number;
  periodLabel: string;
  pfBase: number;
  pfBaseFormatted?: string;
  employeeContributionRate: number;
  employerContributionRate: number;
  employeeContribution: number;
  employeeContributionFormatted?: string;
  employerContribution: number;
  employerContributionFormatted?: string;
  totalContribution: number;
  totalContributionFormatted?: string;
  salaryCalculationId?: number | null;
  subscriptionId: number;
  createdAt: string;
}

export interface PfContributionFilter {
  employeeId?: number;
  branchId?: number;
  year?: number;
  month?: number;
  pageNumber?: number;
  pageSize?: number;
}

export interface PfMonthlyReport {
  year: number;
  month: number;
  monthLabel: string;
  totalEmployees: number;
  totalEmployeeContribution: number;
  totalEmployeeContributionFormatted?: string;
  totalEmployerContribution: number;
  totalEmployerContributionFormatted?: string;
  totalContribution: number;
  totalContributionFormatted?: string;
  contributions: EmployeePfContributionResponse[];
}

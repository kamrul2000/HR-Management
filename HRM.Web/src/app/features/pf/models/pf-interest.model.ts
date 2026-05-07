export interface PfInterestRateResponse {
  id: number;
  fiscalYear: string;
  interestRate: number;
  interestRateLabel?: string;
  effectiveFrom: string;
  effectiveFromFormatted?: string;
  isActive: boolean;
  description?: string | null;
  subscriptionId: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreatePfInterestRateDto {
  fiscalYear: string;
  interestRate: number;
  effectiveFrom: string;
  description?: string | null;
}

export interface ComputePfInterestDto {
  employeeId: number;
  fiscalYear: string;
}

export interface BulkComputePfInterestDto {
  fiscalYear: string;
  branchId?: number | null;
}

export interface EmployeePfInterestResponse {
  id: number;
  employeeId: number;
  employeeCode: string;
  employeeFullName: string;
  pfInterestRateId: number;
  fiscalYear: string;
  interestRate: number;
  interestRateLabel?: string;
  openingBalance: number;
  openingBalanceFormatted?: string;
  totalContributionsForYear: number;
  totalContributionsFormatted?: string;
  averageBalance: number;
  averageBalanceFormatted?: string;
  interestAmount: number;
  interestAmountFormatted?: string;
  closingBalance: number;
  closingBalanceFormatted?: string;
  subscriptionId: number;
  createdAt: string;
  updatedAt: string;
}

export interface PfInterestReport {
  fiscalYear: string;
  interestRate: number;
  interestRateLabel?: string;
  totalEmployees: number;
  totalOpeningBalance: number;
  totalOpeningBalanceFormatted?: string;
  totalContributions: number;
  totalContributionsFormatted?: string;
  totalInterestCredited: number;
  totalInterestCreditedFormatted?: string;
  totalClosingBalance: number;
  totalClosingBalanceFormatted?: string;
  details: EmployeePfInterestResponse[];
}

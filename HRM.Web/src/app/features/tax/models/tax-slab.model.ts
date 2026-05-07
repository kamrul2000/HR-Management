export interface TaxSlab {
  id?: number;
  slabOrder: number;
  minAmount: number;
  minAmountFormatted?: string;
  maxAmount?: number | null;
  maxAmountFormatted?: string;
  taxRate: number;
  taxRateLabel?: string;
  rangeLabel?: string;
}

export interface TaxSlabConfigResponse {
  id: number;
  fiscalYear: string;
  startDate: string;
  startDateFormatted?: string;
  endDate: string;
  endDateFormatted?: string;
  taxFreeThreshold: number;
  taxFreeThresholdFormatted?: string;
  description?: string | null;
  isActive: boolean;
  slabs: TaxSlab[];
  slabCount: number;
  subscriptionId: number;
  createdAt: string;
  updatedAt: string;
}

export interface TaxSlabDto {
  slabOrder: number;
  minAmount: number;
  maxAmount?: number | null;
  taxRate: number;
}

export interface CreateTaxSlabConfigDto {
  fiscalYear: string;
  startDate: string;
  endDate: string;
  taxFreeThreshold: number;
  description?: string | null;
  slabs: TaxSlabDto[];
}

export interface UpdateTaxSlabConfigDto {
  taxFreeThreshold: number;
  description?: string | null;
  isActive: boolean;
  slabs: TaxSlabDto[];
}

export interface ComputeTaxDto {
  annualIncome: number;
  fiscalYear?: string;
}

export interface TaxSlabBreakdown {
  slabOrder: number;
  rangeLabel: string;
  amountInBand: number;
  taxRate: number;
  taxAmount: number;
  taxAmountFormatted?: string;
}

export interface TaxComputationResult {
  annualIncome: number;
  annualIncomeFormatted?: string;
  taxFreeThreshold: number;
  taxableIncome: number;
  annualTax: number;
  annualTaxFormatted?: string;
  monthlyTax: number;
  monthlyTaxFormatted?: string;
  effectiveTaxRate: number;
  effectiveTaxRateLabel?: string;
  fiscalYear: string;
  slabBreakdown: TaxSlabBreakdown[];
}

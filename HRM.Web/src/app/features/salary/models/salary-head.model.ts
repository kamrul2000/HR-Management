export type SalaryHeadType = 'Earning' | 'Deduction';
export type CalculationMethod =
  | 'Fixed'
  | 'PercentageOfBasic'
  | 'PercentageOfGross'
  | 'PercentageOfNet'
  | 'PercentageOfHead';

export interface SalaryHeadResponse {
  id: number;
  headName: string;
  headCode: string;
  headType: SalaryHeadType;
  calculationMethod: CalculationMethod;
  percentage?: number | null;
  baseHeadId?: number | null;
  baseHeadName?: string | null;
  isFixed: boolean;
  isTaxable: boolean;
  isProvidentFundApplicable: boolean;
  displayOrder: number;
  description?: string | null;
  isActive: boolean;
  subscriptionId: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateSalaryHeadDto {
  headName: string;
  headCode: string;
  headType: SalaryHeadType;
  calculationMethod: CalculationMethod;
  percentage?: number | null;
  baseHeadId?: number | null;
  isFixed: boolean;
  isTaxable: boolean;
  isProvidentFundApplicable: boolean;
  displayOrder: number;
  description?: string | null;
}

export interface UpdateSalaryHeadDto extends CreateSalaryHeadDto {
  isActive: boolean;
}

export type TaxExclusionType = 'Full' | 'Partial' | string;

export interface TaxExclusionResponse {
  id: number;
  employeeId: number;
  employeeCode: string;
  employeeFullName: string;
  reason: string;
  exclusionType: TaxExclusionType;
  exclusionTypeLabel?: string;
  partialExclusionAmount?: number | null;
  partialExclusionAmountFormatted?: string | null;
  effectiveFrom: string;
  effectiveFromFormatted?: string;
  effectiveTo?: string | null;
  effectiveToFormatted?: string | null;
  isIndefinite: boolean;
  certificateNo?: string | null;
  attachmentUrl?: string | null;
  isActive: boolean;
  isCurrentlyEffective: boolean;
  subscriptionId: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateTaxExclusionDto {
  employeeId: number;
  reason: string;
  exclusionType: TaxExclusionType;
  partialExclusionAmount?: number | null;
  effectiveFrom: string;
  effectiveTo?: string | null;
  certificateNo?: string | null;
}

export interface UpdateTaxExclusionDto {
  reason: string;
  partialExclusionAmount?: number | null;
  effectiveTo?: string | null;
  certificateNo?: string | null;
  isActive: boolean;
}

export interface TaxExclusionCheck {
  isExcluded: boolean;
  exclusionType: string;
  partialExclusionAmount?: number | null;
}

import { SeparationType } from './separation-reason.model';

export type SeparationStatus = 'Pending' | 'Approved' | 'Processed' | 'Cancelled' | string;

export interface SeparationResponse {
  id: number;
  employeeId: number;
  employeeCode: string;
  employeeFullName: string;
  employeeJoiningDate: string;
  employeeJoiningDateFormatted?: string;
  separationReasonId: number;
  separationReasonName: string;
  separationType: SeparationType;
  separationTypeLabel?: string;
  applicationDate: string;
  applicationDateFormatted?: string;
  lastWorkingDate: string;
  lastWorkingDateFormatted?: string;
  noticePeriodDays: number;
  actualNoticeDays: number;
  noticePeriodShortfall: number;
  noticePeriodShortfallLabel?: string;
  noticePeriodBuyout: number;
  noticePeriodBuyoutFormatted?: string;
  gratuityAmount: number;
  gratuityAmountFormatted?: string;
  otherSettlementAmount: number;
  otherSettlementAmountFormatted?: string;
  totalSettlementAmount: number;
  totalSettlementAmountFormatted?: string;
  remarks?: string | null;
  attachmentUrl?: string | null;
  status: SeparationStatus;
  statusLabel?: string;
  approvedById?: number | null;
  approvalDate?: string | null;
  approvalDateFormatted?: string | null;
  approvalRemarks?: string | null;
  processedDate?: string | null;
  processedDateFormatted?: string | null;
  subscriptionId: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateSeparationDto {
  employeeId: number;
  separationReasonId: number;
  separationType: SeparationType;
  applicationDate: string;
  lastWorkingDate: string;
  noticePeriodDays: number;
  noticePeriodBuyout: number;
  otherSettlementAmount: number;
  remarks?: string | null;
}

export interface ApproveSeparationDto {
  approvalRemarks?: string | null;
}

export interface CancelSeparationDto {
  cancellationReason: string;
}

export interface SeparationFilter {
  separationType?: SeparationType | string;
  status?: SeparationStatus | string;
  branchId?: number;
  fromDate?: string;
  toDate?: string;
  pageNumber?: number;
  pageSize?: number;
}

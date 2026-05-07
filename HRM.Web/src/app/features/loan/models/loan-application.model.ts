export type LoanApplicationStatus =
  | 'Pending'
  | 'Recommended'
  | 'Approved'
  | 'Rejected'
  | 'Disbursed'
  | 'Cancelled';

export type LoanType = 'Personal' | 'Emergency' | 'Salary' | 'Festival' | 'Education' | 'Medical' | string;

export interface LoanApplicationResponse {
  id: number;
  applicationNo: string;
  employeeId: number;
  employeeCode: string;
  employeeFullName: string;
  loanType: string;
  loanTypeLabel?: string;
  requestedAmount: number;
  requestedAmountFormatted?: string;
  requestedTenureMonths: number;
  tenureLabel?: string;
  estimatedMonthlyInstallment: number;
  estimatedMonthlyInstallmentFormatted?: string;
  purpose: string;
  attachmentUrl?: string | null;
  status: LoanApplicationStatus;
  statusLabel?: string;
  recommendedById?: number | null;
  recommendationDate?: string | null;
  recommendationDateFormatted?: string | null;
  recommendationRemarks?: string | null;
  rejectedById?: number | null;
  rejectionDate?: string | null;
  rejectionDateFormatted?: string | null;
  rejectionRemarks?: string | null;
  subscriptionId: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateLoanApplicationDto {
  employeeId: number;
  loanType: string;
  requestedAmount: number;
  requestedTenureMonths: number;
  purpose: string;
}

export interface CancelLoanApplicationDto {
  cancellationReason: string;
}

export interface LoanApplicationFilter {
  employeeId?: number;
  branchId?: number;
  loanType?: string;
  status?: LoanApplicationStatus | string;
  fromDate?: string;
  toDate?: string;
  pageNumber?: number;
  pageSize?: number;
}

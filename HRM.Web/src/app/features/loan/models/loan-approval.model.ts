export type ApprovalDecision = 'Approve' | 'Reject';
export type InterestType = 'Flat' | 'Reducing' | 'None' | string;

export interface LoanApprovalResponse {
  id: number;
  loanApplicationId: number;
  applicationNo: string;
  employeeId: number;
  employeeCode: string;
  employeeFullName: string;
  requestedAmount: number;
  recommendedAmount?: number | null;
  approvedById: number;
  decision: ApprovalDecision | string;
  decisionLabel?: string;
  approvedAmount?: number | null;
  approvedAmountFormatted?: string | null;
  approvedTenureMonths?: number | null;
  tenureLabel?: string | null;
  interestRate?: number | null;
  interestType?: InterestType | null;
  interestTypeLabel?: string | null;
  monthlyInstallment?: number | null;
  monthlyInstallmentFormatted?: string | null;
  totalRepayable?: number | null;
  totalRepayableFormatted?: string | null;
  remarks: string;
  subscriptionId: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateLoanApprovalDto {
  loanApplicationId: number;
  decision: ApprovalDecision;
  approvedAmount?: number | null;
  approvedTenureMonths?: number | null;
  interestRate?: number | null;
  interestType?: InterestType | null;
  remarks: string;
}

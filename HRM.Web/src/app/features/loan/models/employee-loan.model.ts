import { LoanInstallmentStatus } from './loan-installment.model';

export type EmployeeLoanStatus = 'Active' | 'Completed' | 'Defaulted' | 'Cancelled';

export interface LoanInstallmentSummary {
  id: number;
  installmentNo: number;
  dueMonth: number;
  dueYear: number;
  duePeriodLabel: string;
  installmentAmount: number;
  installmentAmountFormatted?: string;
  paidAmount: number;
  status: LoanInstallmentStatus | string;
  statusLabel?: string;
  paidDate?: string | null;
}

export interface EmployeeLoanResponse {
  id: number;
  loanNo: string;
  loanApplicationId: number;
  applicationNo: string;
  employeeId: number;
  employeeCode: string;
  employeeFullName: string;
  loanType: string;
  principalAmount: number;
  principalAmountFormatted?: string;
  interestRate: number;
  interestType?: string | null;
  interestTypeLabel?: string;
  tenureMonths: number;
  tenureLabel?: string;
  monthlyInstallment: number;
  monthlyInstallmentFormatted?: string;
  totalRepayable: number;
  totalRepayableFormatted?: string;
  disbursementDate: string;
  disbursementDateFormatted?: string;
  firstInstallmentMonth: number;
  firstInstallmentYear: number;
  firstInstallmentPeriodLabel?: string;
  totalPaid: number;
  totalPaidFormatted?: string;
  outstandingBalance: number;
  outstandingBalanceFormatted?: string;
  paidInstallments: number;
  remainingInstallments: number;
  completionPercentage: number;
  status: EmployeeLoanStatus | string;
  statusLabel?: string;
  remarks?: string | null;
  installments: LoanInstallmentSummary[];
  subscriptionId: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateEmployeeLoanDto {
  loanApplicationId: number;
  disbursementDate: string;
  firstInstallmentMonth: number;
  firstInstallmentYear: number;
  remarks?: string | null;
}

export interface EmployeeLoanFilter {
  employeeId?: number;
  branchId?: number;
  status?: EmployeeLoanStatus | string;
  loanType?: string;
  pageNumber?: number;
  pageSize?: number;
}

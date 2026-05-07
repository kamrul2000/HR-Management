export type LoanInstallmentStatus = 'Pending' | 'Paid' | 'Skipped' | 'Overdue' | 'Cancelled';

export interface LoanInstallmentResponse {
  id: number;
  employeeLoanId: number;
  loanNo: string;
  employeeId: number;
  employeeCode: string;
  employeeFullName: string;
  installmentNo: number;
  dueMonth: number;
  dueYear: number;
  duePeriodLabel: string;
  installmentAmount: number;
  installmentAmountFormatted?: string;
  paidAmount: number;
  paidAmountFormatted?: string;
  paidDate?: string | null;
  paidDateFormatted?: string | null;
  status: LoanInstallmentStatus | string;
  statusLabel?: string;
  remarks?: string | null;
  salaryCalculationId?: number | null;
  subscriptionId: number;
  createdAt: string;
  updatedAt: string;
}

export interface ProcessInstallmentDto {
  paidAmount?: number | null;
  salaryCalculationId?: number | null;
  remarks?: string | null;
}

export interface SkipInstallmentDto {
  reason: string;
}

export interface PendingInstallment {
  installmentId: number;
  employeeLoanId: number;
  installmentAmount: number;
  installmentNo: number;
  dueMonth: number;
  dueYear: number;
}

export interface InstallmentFilter {
  employeeId?: number;
  employeeLoanId?: number;
  status?: LoanInstallmentStatus | string;
  dueMonth?: number;
  dueYear?: number;
  pageNumber?: number;
  pageSize?: number;
}

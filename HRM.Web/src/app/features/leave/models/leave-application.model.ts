export type LeaveApplicationStatus =
  | 'Pending'
  | 'Approved'
  | 'Rejected'
  | 'Cancelled';

export interface LeaveApplicationResponse {
  id: number;
  applicationNo: string;
  employeeId: number;
  employeeFullName?: string;
  employeeCode?: string;
  leaveTypeId: number;
  leaveTypeName?: string;
  leaveAllotmentId: number;
  fromDate: string;
  toDate: string;
  fromDateFormatted?: string;
  toDateFormatted?: string;
  totalDays: number;
  reason: string;
  attachmentPath?: string | null;
  attachmentUrl?: string | null;
  status: LeaveApplicationStatus;
  statusLabel?: string;
  approvedById?: number | null;
  approvedAt?: string | null;
  approvalRemarks?: string | null;
  cancellationReason?: string | null;
  subscriptionId: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateLeaveApplicationDto {
  employeeId: number;
  leaveTypeId: number;
  fromDate: string;
  toDate: string;
  reason: string;
}

export interface ApproveLeaveApplicationDto {
  approvalRemarks?: string | null;
}

export interface RejectLeaveApplicationDto {
  approvalRemarks: string;
}

export interface CancelLeaveApplicationDto {
  cancellationReason: string;
}

export interface LeaveApplicationFilter {
  employeeId?: number;
  leaveTypeId?: number;
  status?: LeaveApplicationStatus | string;
  fromDate?: string;
  toDate?: string;
  branchId?: number;
  search?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface WorkingDaysResult {
  totalDays: number;
  workingDays: number;
  holidayCount: number;
  offDayCount: number;
}

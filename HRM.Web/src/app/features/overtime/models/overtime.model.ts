export type OvertimeStatus = 'Pending' | 'Approved' | 'Rejected';
export type OvertimeType = 'Regular' | 'Holiday' | 'WeeklyOff';

export interface OvertimeResponse {
  id: number;
  employeeId: number;
  employeeFullName?: string;
  employeeCode?: string;
  attendanceId: number;
  attendanceDate: string;
  attendanceDateFormatted?: string;
  overtimeDate: string;
  overtimeDateFormatted?: string;
  requestedMinutes: number;
  approvedMinutes: number;
  overtimeType: OvertimeType;
  reason: string;
  status: OvertimeStatus;
  statusLabel?: string;
  approvalRemarks?: string | null;
  approvedAt?: string | null;
  approvedById?: number | null;
  subscriptionId: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateOvertimeDto {
  employeeId: number;
  attendanceId: number;
  overtimeDate: string;
  requestedMinutes: number;
  overtimeType: OvertimeType;
  reason: string;
}

export interface ApproveOvertimeDto {
  approvedMinutes?: number;
  approvalRemarks?: string | null;
}

export interface RejectOvertimeDto {
  approvalRemarks: string;
}

export interface OvertimeFilter {
  search?: string;
  employeeId?: number;
  branchId?: number;
  status?: OvertimeStatus | string;
  overtimeType?: OvertimeType | string;
  year?: number;
  month?: number;
  pageNumber?: number;
  pageSize?: number;
}

export interface OvertimeSummary {
  employeeId: number;
  employeeFullName?: string;
  employeeCode?: string;
  year: number;
  month: number;
  totalRequestedMinutes: number;
  totalApprovedMinutes: number;
  regularMinutes: number;
  holidayMinutes: number;
  weeklyOffMinutes: number;
  pendingCount: number;
  approvedCount: number;
  rejectedCount: number;
}

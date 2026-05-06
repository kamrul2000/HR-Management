export type AttendanceStatus =
  | 'Present'
  | 'Absent'
  | 'Late'
  | 'HalfDay'
  | 'Holiday'
  | 'WeeklyOff'
  | 'OnLeave';

export interface AttendanceResponse {
  id: number;
  employeeId: number;
  employeeFullName?: string;
  employeeCode?: string;
  dutySlotId: number;
  dutySlotName?: string;
  attendanceDate: string;
  attendanceDateFormatted?: string;
  punchInTime?: string | null;
  punchOutTime?: string | null;
  punchInTimeFormatted?: string | null;
  punchOutTimeFormatted?: string | null;
  status: AttendanceStatus;
  statusLabel?: string;
  isLate: boolean;
  lateMinutes: number;
  actualWorkingMinutes: number;
  scheduledWorkingMinutes: number;
  overtimeMinutes: number;
  workingHoursLabel?: string;
  remarks?: string | null;
  subscriptionId: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateAttendanceDto {
  employeeId: number;
  dutySlotId: number;
  attendanceDate: string;
  punchInTime?: string | null;
  punchOutTime?: string | null;
  status: AttendanceStatus;
  remarks?: string | null;
}

export interface UpdateAttendanceDto extends CreateAttendanceDto {}

export interface BulkAttendanceRow {
  employeeId: number;
  punchInTime?: string | null;
  punchOutTime?: string | null;
  status: AttendanceStatus;
  remarks?: string | null;
}

export interface BulkAttendanceDto {
  attendanceDate: string;
  dutySlotId: number;
  rows: BulkAttendanceRow[];
}

export interface AttendanceFilter {
  search?: string;
  branchId?: number;
  departmentId?: number;
  employeeId?: number;
  status?: AttendanceStatus | string;
  fromDate?: string;
  toDate?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface MonthlyAttendanceSummary {
  employeeId: number;
  employeeFullName?: string;
  year: number;
  month: number;
  presentDays: number;
  absentDays: number;
  lateDays: number;
  halfDays: number;
  holidayDays: number;
  weeklyOffDays: number;
  leaveDays: number;
  totalWorkingMinutes: number;
  totalOvertimeMinutes: number;
  daily: DailyAttendanceCell[];
}

export interface DailyAttendanceCell {
  day: number;
  date: string;
  status: AttendanceStatus | 'NoRecord';
  isLate: boolean;
  punchInTimeFormatted?: string | null;
  punchOutTimeFormatted?: string | null;
}

export interface AttendanceSummary {
  present: number;
  absent: number;
  late: number;
  halfDay: number;
  holiday: number;
  weeklyOff: number;
  total: number;
}

export interface DepartmentHeadcount {
  departmentName: string;
  count: number;
}

export interface RecentLeaveRow {
  id: number;
  employeeFullName: string;
  employeeCode: string;
  leaveTypeName: string;
  fromDate: string;
  toDate: string;
  fromDateFormatted: string;
  toDateFormatted: string;
  totalDays: number;
  status: string;
  appliedAt: string;
}

export interface RecentSalaryRow {
  id: number;
  employeeFullName: string;
  employeeCode: string;
  monthLabel: string;
  netSalary: number;
  netSalaryFormatted: string;
  status: string;
  createdAt: string;
}

/** Backend filter shapes — mirroring the .NET DTOs we hit from this dashboard. */
export interface AttendanceRow {
  id: number;
  employeeId: number;
  attendanceDate: string;
  status: string;
  isLate: boolean;
}

export interface EmployeeListRow {
  id: number;
  branchId?: number;
  departmentId?: number;
  departmentName?: string;
  fullName: string;
  status: string;
  isActive: boolean;
}

export interface LeaveApplicationRow {
  id: number;
  employeeFullName?: string;
  employeeCode?: string;
  leaveTypeName?: string;
  fromDate: string;
  toDate: string;
  fromDateFormatted?: string;
  toDateFormatted?: string;
  totalDays: number;
  status: string;
  createdAt: string;
}

export interface SalaryCalculationRow {
  id: number;
  employeeFullName?: string;
  employeeCode?: string;
  monthLabel?: string;
  year: number;
  month: number;
  netSalary: number;
  netSalaryFormatted?: string;
  status: string;
  createdAt: string;
}

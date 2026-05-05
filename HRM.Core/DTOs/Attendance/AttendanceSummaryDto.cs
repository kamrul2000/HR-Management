namespace HRM.Core.DTOs.Attendance;

public class AttendanceSummaryDto
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeFullName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public int TotalWorkingDays { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int HalfDays { get; set; }
    public int LateDays { get; set; }
    public int HolidayDays { get; set; }
    public int WeeklyOffDays { get; set; }
    public int TotalLateMinutes { get; set; }
    public int TotalOvertimeMinutes { get; set; }
    public int TotalActualWorkingMinutes { get; set; }
    public decimal AttendancePercentage { get; set; }
}

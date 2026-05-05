namespace HRM.Core.DTOs.Overtime;

public class OvertimeSummaryDto
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeFullName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public int TotalRequestedMinutes { get; set; }
    public int TotalApprovedMinutes { get; set; }
    public int RegularOvertimeMinutes { get; set; }
    public int HolidayOvertimeMinutes { get; set; }
    public int WeeklyOffOvertimeMinutes { get; set; }
    public int TotalApprovedRecords { get; set; }
    public string TotalApprovedFormatted { get; set; } = string.Empty;
}

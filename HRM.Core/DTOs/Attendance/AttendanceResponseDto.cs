namespace HRM.Core.DTOs.Attendance;

public class AttendanceResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeFullName { get; set; } = string.Empty;
    public int DutySlotId { get; set; }
    public string SlotName { get; set; } = string.Empty;
    public string ShiftTime { get; set; } = string.Empty;
    public DateTime AttendanceDate { get; set; }
    public string AttendanceDateFormatted { get; set; } = string.Empty;
    public TimeSpan? PunchInTime { get; set; }
    public string? PunchInFormatted { get; set; }
    public TimeSpan? PunchOutTime { get; set; }
    public string? PunchOutFormatted { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public bool IsLate { get; set; }
    public int LateMinutes { get; set; }
    public string LateFormatted { get; set; } = string.Empty;
    public int ActualWorkingMinutes { get; set; }
    public string ActualWorkingHoursFormatted { get; set; } = string.Empty;
    public int ScheduledWorkingMinutes { get; set; }
    public int OvertimeMinutes { get; set; }
    public string OvertimeFormatted { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public int SubscriptionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

namespace HRM.Core.DTOs.Overtime;

public class OvertimeResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeFullName { get; set; } = string.Empty;
    public int AttendanceId { get; set; }
    public DateTime OvertimeDate { get; set; }
    public string OvertimeDateFormatted { get; set; } = string.Empty;
    public int RequestedMinutes { get; set; }
    public string RequestedFormatted { get; set; } = string.Empty;
    public int ApprovedMinutes { get; set; }
    public string ApprovedFormatted { get; set; } = string.Empty;
    public string OvertimeType { get; set; } = string.Empty;
    public string OvertimeTypeLabel { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public int? ApprovedById { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public string? ApprovalDateFormatted { get; set; }
    public string? ApprovalRemarks { get; set; }
    public int SubscriptionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

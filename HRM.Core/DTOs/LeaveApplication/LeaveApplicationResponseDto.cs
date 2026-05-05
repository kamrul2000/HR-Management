namespace HRM.Core.DTOs.LeaveApplication;

public class LeaveApplicationResponseDto
{
    public int Id { get; set; }
    public string ApplicationNo { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeFullName { get; set; } = string.Empty;
    public int LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public string LeaveTypeCode { get; set; } = string.Empty;
    public bool IsPaid { get; set; }
    public int LeaveAllotmentId { get; set; }
    public decimal RemainingBalanceAfter { get; set; }
    public DateTime FromDate { get; set; }
    public string FromDateFormatted { get; set; } = string.Empty;
    public DateTime ToDate { get; set; }
    public string ToDateFormatted { get; set; } = string.Empty;
    public decimal TotalDays { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public int? ApprovedById { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public string? ApprovalDateFormatted { get; set; }
    public string? ApprovalRemarks { get; set; }
    public string? CancellationReason { get; set; }
    public int SubscriptionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

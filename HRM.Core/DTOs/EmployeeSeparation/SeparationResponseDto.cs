namespace HRM.Core.DTOs.EmployeeSeparation;

public class SeparationResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeFullName { get; set; } = string.Empty;
    public DateTime EmployeeJoiningDate { get; set; }
    public string EmployeeJoiningDateFormatted { get; set; } = string.Empty;
    public int SeparationReasonId { get; set; }
    public string SeparationReasonName { get; set; } = string.Empty;
    public string SeparationType { get; set; } = string.Empty;
    public string SeparationTypeLabel { get; set; } = string.Empty;
    public DateTime ApplicationDate { get; set; }
    public string ApplicationDateFormatted { get; set; } = string.Empty;
    public DateTime LastWorkingDate { get; set; }
    public string LastWorkingDateFormatted { get; set; } = string.Empty;
    public int NoticePeriodDays { get; set; }
    public int ActualNoticeDays { get; set; }
    public int NoticePeriodShortfall { get; set; }
    public string NoticePeriodShortfallLabel { get; set; } = string.Empty;
    public decimal NoticePeriodBuyout { get; set; }
    public string NoticePeriodBuyoutFormatted { get; set; } = string.Empty;
    public decimal GratuityAmount { get; set; }
    public string GratuityAmountFormatted { get; set; } = string.Empty;
    public decimal OtherSettlementAmount { get; set; }
    public string OtherSettlementAmountFormatted { get; set; } = string.Empty;
    public decimal TotalSettlementAmount { get; set; }
    public string TotalSettlementAmountFormatted { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public string? AttachmentUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public int? ApprovedById { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public string? ApprovalDateFormatted { get; set; }
    public string? ApprovalRemarks { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public string? ProcessedDateFormatted { get; set; }
    public int SubscriptionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

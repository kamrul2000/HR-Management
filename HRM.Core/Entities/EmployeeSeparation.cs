using System.ComponentModel.DataAnnotations;

namespace HRM.Core.Entities;

public class EmployeeSeparation
{
    public int Id { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    public Employee Employee { get; set; } = null!;

    [Required]
    public int SeparationReasonId { get; set; }

    public SeparationReason SeparationReason { get; set; } = null!;

    [Required]
    [MaxLength(30)]
    public string SeparationType { get; set; } = string.Empty;

    [Required]
    public DateTime ApplicationDate { get; set; }

    [Required]
    public DateTime LastWorkingDate { get; set; }

    [Required]
    [Range(0, 365)]
    public int NoticePeriodDays { get; set; }

    [Required]
    public int ActualNoticeDays { get; set; }

    [Required]
    public int NoticePeriodShortfall { get; set; }

    public decimal NoticePeriodBuyout { get; set; }

    public decimal GratuityAmount { get; set; }

    public decimal OtherSettlementAmount { get; set; }

    [Required]
    public decimal TotalSettlementAmount { get; set; }

    [MaxLength(1000)]
    public string? Remarks { get; set; }

    [MaxLength(500)]
    public string? AttachmentPath { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Draft";

    public int? ApprovedById { get; set; }

    public DateTime? ApprovalDate { get; set; }

    [MaxLength(500)]
    public string? ApprovalRemarks { get; set; }

    public DateTime? ProcessedDate { get; set; }

    [Required]
    public int SubscriptionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

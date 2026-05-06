namespace HRM.Core.DTOs.TaxExclusion;

public class TaxExclusionResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeFullName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string ExclusionType { get; set; } = string.Empty;
    public string ExclusionTypeLabel { get; set; } = string.Empty;
    public decimal? PartialExclusionAmount { get; set; }
    public string? PartialExclusionAmountFormatted { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public string EffectiveFromFormatted { get; set; } = string.Empty;
    public DateTime? EffectiveTo { get; set; }
    public string? EffectiveToFormatted { get; set; }
    public bool IsIndefinite { get; set; }
    public string? CertificateNo { get; set; }
    public string? AttachmentUrl { get; set; }
    public bool IsActive { get; set; }
    public bool IsCurrentlyEffective { get; set; }
    public int SubscriptionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

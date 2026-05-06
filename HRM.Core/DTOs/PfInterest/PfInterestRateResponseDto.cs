namespace HRM.Core.DTOs.PfInterest;

public class PfInterestRateResponseDto
{
    public int Id { get; set; }
    public string FiscalYear { get; set; } = string.Empty;
    public decimal InterestRate { get; set; }
    public string InterestRateLabel { get; set; } = string.Empty;
    public DateTime EffectiveFrom { get; set; }
    public string EffectiveFromFormatted { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? Description { get; set; }
    public int SubscriptionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

namespace HRM.Core.DTOs.GratuitySetup;

public class GratuityRuleResponseDto
{
    public int Id { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public decimal MinServiceYears { get; set; }
    public string MinServiceYearsLabel { get; set; } = string.Empty;
    public string CalculationBasis { get; set; } = string.Empty;
    public string CalculationBasisLabel { get; set; } = string.Empty;
    public decimal RatePerYear { get; set; }
    public string RatePerYearLabel { get; set; } = string.Empty;
    public decimal? MaxGratuityAmount { get; set; }
    public string? MaxGratuityAmountFormatted { get; set; }
    public int? MaxServiceYearsCapped { get; set; }
    public string? MaxServiceYearsCappedLabel { get; set; }
    public bool ProRataEnabled { get; set; }
    public string ProRataLabel { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public string EffectiveFromFormatted { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SubscriptionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

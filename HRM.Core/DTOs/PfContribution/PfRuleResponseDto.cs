namespace HRM.Core.DTOs.PfContribution;

public class PfRuleResponseDto
{
    public int Id { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public decimal EmployeeContributionRate { get; set; }
    public string EmployeeContributionRateLabel { get; set; } = string.Empty;
    public decimal EmployerContributionRate { get; set; }
    public string EmployerContributionRateLabel { get; set; } = string.Empty;
    public string PfBase { get; set; } = string.Empty;
    public string PfBaseLabel { get; set; } = string.Empty;
    public decimal? MinEligibleSalary { get; set; }
    public string? MinEligibleSalaryFormatted { get; set; }
    public decimal? MaxContributionAmount { get; set; }
    public string? MaxContributionAmountFormatted { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public string EffectiveFromFormatted { get; set; } = string.Empty;
    public DateTime? EffectiveTo { get; set; }
    public string? EffectiveToFormatted { get; set; }
    public bool IsActive { get; set; }
    public string? Description { get; set; }
    public int SubscriptionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

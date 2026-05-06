using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.PfContribution;

public class UpdatePfRuleDto
{
    [Required]
    [MaxLength(100)]
    public string RuleName { get; set; } = string.Empty;

    [Required]
    [Range(typeof(decimal), "0.01", "100")]
    public decimal EmployeeContributionRate { get; set; }

    [Required]
    [Range(typeof(decimal), "0", "100")]
    public decimal EmployerContributionRate { get; set; }

    [Range(typeof(decimal), "0", "9999999.99")]
    public decimal? MinEligibleSalary { get; set; }

    [Range(typeof(decimal), "0.01", "9999999.99")]
    public decimal? MaxContributionAmount { get; set; }

    public DateTime? EffectiveTo { get; set; }

    [Required]
    public bool IsActive { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
}

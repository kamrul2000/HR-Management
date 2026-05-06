using System.ComponentModel.DataAnnotations;

namespace HRM.Core.Entities;

public class PfRule
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string RuleName { get; set; } = string.Empty;

    [Required]
    [Range(typeof(decimal), "0.01", "100")]
    public decimal EmployeeContributionRate { get; set; }

    [Required]
    [Range(typeof(decimal), "0", "100")]
    public decimal EmployerContributionRate { get; set; }

    [Required]
    [MaxLength(30)]
    public string PfBase { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "9999999.99")]
    public decimal? MinEligibleSalary { get; set; }

    [Range(typeof(decimal), "0.01", "9999999.99")]
    public decimal? MaxContributionAmount { get; set; }

    [Required]
    public DateTime EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public int SubscriptionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

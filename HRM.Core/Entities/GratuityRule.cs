using System.ComponentModel.DataAnnotations;

namespace HRM.Core.Entities;

public class GratuityRule
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string RuleName { get; set; } = string.Empty;

    [Required]
    [Range(typeof(decimal), "0.5", "10")]
    public decimal MinServiceYears { get; set; }

    [Required]
    [MaxLength(30)]
    public string CalculationBasis { get; set; } = string.Empty;

    [Required]
    [Range(typeof(decimal), "0.01", "60")]
    public decimal RatePerYear { get; set; }

    [Range(typeof(decimal), "1", "99999999.99")]
    public decimal? MaxGratuityAmount { get; set; }

    [Range(1, 40)]
    public int? MaxServiceYearsCapped { get; set; }

    public bool ProRataEnabled { get; set; } = true;

    public bool IsActive { get; set; } = true;

    [Required]
    public DateTime EffectiveFrom { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public int SubscriptionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

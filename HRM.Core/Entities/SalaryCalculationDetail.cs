using System.ComponentModel.DataAnnotations;

namespace HRM.Core.Entities;

public class SalaryCalculationDetail
{
    public int Id { get; set; }

    [Required]
    public int SalaryCalculationId { get; set; }

    public SalaryCalculation SalaryCalculation { get; set; } = null!;

    [Required]
    public int SalaryHeadId { get; set; }

    [Required]
    [MaxLength(100)]
    public string HeadName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string HeadCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string HeadType { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string CalculationMethod { get; set; } = string.Empty;

    public decimal? BaseAmount { get; set; }

    public decimal? AppliedPercentage { get; set; }

    [Required]
    public decimal ComputedAmount { get; set; }

    public int DisplayOrder { get; set; }

    [Required]
    public int SubscriptionId { get; set; }

    public DateTime CreatedAt { get; set; }
}

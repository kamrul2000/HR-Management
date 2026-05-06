using System.ComponentModel.DataAnnotations;

namespace HRM.Core.Entities;

public class SalaryStructureItem
{
    public int Id { get; set; }

    [Required]
    public int SalaryStructureId { get; set; }

    public SalaryStructure SalaryStructure { get; set; } = null!;

    [Required]
    public int SalaryHeadId { get; set; }

    public SalaryHead SalaryHead { get; set; } = null!;

    [Range(typeof(decimal), "0", "9999999.99")]
    public decimal? FixedAmount { get; set; }

    [Range(typeof(decimal), "0.01", "100")]
    public decimal? OverridePercentage { get; set; }

    [Required]
    public int SubscriptionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

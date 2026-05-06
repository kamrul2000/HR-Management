using System.ComponentModel.DataAnnotations;

namespace HRM.Core.Entities;

public class SalaryHead
{
    public int Id { get; set; }

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

    [Range(typeof(decimal), "0.01", "100")]
    public decimal? Percentage { get; set; }

    public int? BaseHeadId { get; set; }

    public SalaryHead? BaseHead { get; set; }

    public ICollection<SalaryHead> DependentHeads { get; set; } = new List<SalaryHead>();

    public bool IsFixed { get; set; }

    public bool IsTaxable { get; set; } = true;

    public bool IsProvidentFundApplicable { get; set; }

    [Required]
    [Range(1, 999)]
    public int DisplayOrder { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public int SubscriptionId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

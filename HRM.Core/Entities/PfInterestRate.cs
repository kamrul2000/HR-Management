using System.ComponentModel.DataAnnotations;

namespace HRM.Core.Entities;

public class PfInterestRate
{
    public int Id { get; set; }

    [Required]
    [MaxLength(10)]
    public string FiscalYear { get; set; } = string.Empty;

    [Required]
    [Range(typeof(decimal), "0.01", "30")]
    public decimal InterestRate { get; set; }

    [Required]
    public DateTime EffectiveFrom { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public int SubscriptionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.PfInterest;

public class CreatePfInterestRateDto
{
    [Required]
    [MaxLength(10)]
    public string FiscalYear { get; set; } = string.Empty;

    [Required]
    [Range(typeof(decimal), "0.01", "30")]
    public decimal InterestRate { get; set; }

    [Required]
    public DateTime EffectiveFrom { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.SalaryHead;

public class UpdateSalaryHeadDto
{
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

    [Range(1, int.MaxValue)]
    public int? BaseHeadId { get; set; }

    [Required]
    public bool IsFixed { get; set; }

    [Required]
    public bool IsTaxable { get; set; }

    [Required]
    public bool IsProvidentFundApplicable { get; set; }

    [Required]
    [Range(1, 999)]
    public int DisplayOrder { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public bool IsActive { get; set; }
}

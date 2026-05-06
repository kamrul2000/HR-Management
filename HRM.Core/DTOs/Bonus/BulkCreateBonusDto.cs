using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.Bonus;

public class BulkCreateBonusDto
{
    [Required]
    [MinLength(1)]
    public List<int> EmployeeIds { get; set; } = new();

    [Required]
    [MaxLength(50)]
    public string BonusType { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string BonusTitle { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string CalculationBasis { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "500")]
    public decimal? BasisPercentage { get; set; }

    [Range(typeof(decimal), "0.01", "9999999.99")]
    public decimal? FixedAmount { get; set; }

    [Required]
    [Range(1, 12)]
    public int DisbursementMonth { get; set; }

    [Required]
    [Range(2000, 2100)]
    public int DisbursementYear { get; set; }

    public bool IsDisbursedWithSalary { get; set; } = true;

    [MaxLength(500)]
    public string? Remarks { get; set; }
}

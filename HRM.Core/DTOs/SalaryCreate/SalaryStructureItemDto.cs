using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.SalaryCreate;

public class SalaryStructureItemDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int SalaryHeadId { get; set; }

    /// <summary>Required when SalaryHead.CalculationMethod == "Fixed".</summary>
    [Range(typeof(decimal), "0", "9999999.99")]
    public decimal? FixedAmount { get; set; }

    /// <summary>Optional per-employee percentage override (replaces head's default Percentage).</summary>
    [Range(typeof(decimal), "0.01", "100")]
    public decimal? OverridePercentage { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.TaxSlab;

public class ComputeTaxDto
{
    [Required]
    [Range(typeof(decimal), "0", "9999999999.99")]
    public decimal AnnualIncome { get; set; }

    /// <summary>Fiscal year string e.g. "2024-2025". Uses active config if null.</summary>
    [MaxLength(10)]
    public string? FiscalYear { get; set; }
}

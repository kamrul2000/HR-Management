using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.LoanInstallment;

public class ProcessInstallmentDto
{
    /// <summary>Actual amount paid — defaults to InstallmentAmount if null.</summary>
    [Range(typeof(decimal), "0.01", "9999999.99")]
    public decimal? PaidAmount { get; set; }

    /// <summary>Optional: link to a salary calculation record.</summary>
    [Range(1, int.MaxValue)]
    public int? SalaryCalculationId { get; set; }

    [MaxLength(500)]
    public string? Remarks { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.EmployeeLoan;

public class CreateEmployeeLoanDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int LoanApplicationId { get; set; }

    [Required]
    public DateTime DisbursementDate { get; set; }

    /// <summary>Month in which first installment will be deducted (1–12).</summary>
    [Required]
    [Range(1, 12)]
    public int FirstInstallmentMonth { get; set; }

    [Required]
    [Range(2000, 2100)]
    public int FirstInstallmentYear { get; set; }

    [MaxLength(500)]
    public string? Remarks { get; set; }
}

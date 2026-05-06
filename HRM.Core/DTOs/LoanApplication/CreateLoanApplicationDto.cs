using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.LoanApplication;

public class CreateLoanApplicationDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int EmployeeId { get; set; }

    [Required]
    [MaxLength(50)]
    public string LoanType { get; set; } = string.Empty;

    [Required]
    [Range(typeof(decimal), "1", "9999999.99")]
    public decimal RequestedAmount { get; set; }

    [Required]
    [Range(1, 120)]
    public int RequestedTenureMonths { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Purpose { get; set; } = string.Empty;
}

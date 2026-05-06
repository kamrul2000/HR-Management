using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.LoanApplication;

public class CancelLoanApplicationDto
{
    [Required]
    [MaxLength(500)]
    public string CancellationReason { get; set; } = string.Empty;
}

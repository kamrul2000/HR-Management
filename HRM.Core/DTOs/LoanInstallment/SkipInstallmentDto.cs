using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.LoanInstallment;

public class SkipInstallmentDto
{
    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}

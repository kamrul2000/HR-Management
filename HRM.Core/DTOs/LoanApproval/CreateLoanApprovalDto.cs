using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.LoanApproval;

public class CreateLoanApprovalDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int LoanApplicationId { get; set; }

    [Required]
    [MaxLength(20)]
    public string Decision { get; set; } = string.Empty;

    [Range(typeof(decimal), "1", "9999999.99")]
    public decimal? ApprovedAmount { get; set; }

    [Range(1, 120)]
    public int? ApprovedTenureMonths { get; set; }

    [Range(typeof(decimal), "0", "100")]
    public decimal? InterestRate { get; set; }

    [MaxLength(20)]
    public string? InterestType { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Remarks { get; set; } = string.Empty;
}

using System.ComponentModel.DataAnnotations;

namespace HRM.Core.Entities;

public class LoanApproval
{
    public int Id { get; set; }

    [Required]
    public int LoanApplicationId { get; set; }

    public LoanApplication LoanApplication { get; set; } = null!;

    [Required]
    public int ApprovedById { get; set; }

    [Required]
    [MaxLength(20)]
    public string Decision { get; set; } = string.Empty;

    [Range(typeof(decimal), "1", "9999999.99")]
    public decimal? ApprovedAmount { get; set; }

    [Range(1, 120)]
    public int? ApprovedTenureMonths { get; set; }

    public decimal? MonthlyInstallment { get; set; }

    [Range(typeof(decimal), "0", "100")]
    public decimal? InterestRate { get; set; }

    [MaxLength(20)]
    public string? InterestType { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Remarks { get; set; } = string.Empty;

    [Required]
    public int SubscriptionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

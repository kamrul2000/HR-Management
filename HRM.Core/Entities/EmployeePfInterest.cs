using System.ComponentModel.DataAnnotations;

namespace HRM.Core.Entities;

public class EmployeePfInterest
{
    public int Id { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    public Employee Employee { get; set; } = null!;

    [Required]
    public int PfInterestRateId { get; set; }

    public PfInterestRate PfInterestRate { get; set; } = null!;

    [Required]
    [MaxLength(10)]
    public string FiscalYear { get; set; } = string.Empty;

    [Required]
    public decimal OpeningBalance { get; set; }

    [Required]
    public decimal TotalContributionsForYear { get; set; }

    [Required]
    public decimal AverageBalance { get; set; }

    [Required]
    public decimal InterestRate { get; set; }

    [Required]
    public decimal InterestAmount { get; set; }

    [Required]
    public decimal ClosingBalance { get; set; }

    [Required]
    public int SubscriptionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

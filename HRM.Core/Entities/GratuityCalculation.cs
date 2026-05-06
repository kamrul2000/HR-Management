using System.ComponentModel.DataAnnotations;

namespace HRM.Core.Entities;

public class GratuityCalculation
{
    public int Id { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    public Employee Employee { get; set; } = null!;

    [Required]
    public int GratuityRuleId { get; set; }

    public GratuityRule GratuityRule { get; set; } = null!;

    [Required]
    public DateTime SeparationDate { get; set; }

    [Required]
    public DateTime JoiningDate { get; set; }

    [Required]
    public int TotalServiceDays { get; set; }

    [Required]
    public decimal TotalServiceYears { get; set; }

    [Required]
    public decimal EligibleYears { get; set; }

    [Required]
    [MaxLength(30)]
    public string CalculationBasis { get; set; } = string.Empty;

    [Required]
    public decimal MonthlySalaryUsed { get; set; }

    [Required]
    public decimal DailySalary { get; set; }

    [Required]
    public decimal RatePerYear { get; set; }

    [Required]
    public decimal GratuityBeforeCap { get; set; }

    [Required]
    public decimal GratuityAmount { get; set; }

    [Required]
    public bool IsCapApplied { get; set; }

    [Required]
    public bool IsEligible { get; set; }

    [MaxLength(500)]
    public string? IneligibilityReason { get; set; }

    public int? SeparationId { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Draft";

    [MaxLength(500)]
    public string? Remarks { get; set; }

    [Required]
    public int SubscriptionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

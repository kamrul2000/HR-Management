using System.ComponentModel.DataAnnotations;

namespace HRM.Core.Entities;

public class BonusCalculation
{
    public int Id { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    public Employee Employee { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string BonusType { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string BonusTitle { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string CalculationBasis { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "500")]
    public decimal? BasisPercentage { get; set; }

    [Required]
    public decimal BasicSalarySnapshot { get; set; }

    [Required]
    public decimal GrossSalarySnapshot { get; set; }

    [Required]
    public decimal ComputedAmount { get; set; }

    [Required]
    public decimal FinalAmount { get; set; }

    [Required]
    [Range(1, 12)]
    public int DisbursementMonth { get; set; }

    [Required]
    [Range(2000, 2100)]
    public int DisbursementYear { get; set; }

    public bool IsDisbursedWithSalary { get; set; } = true;

    public int? SalaryCalculationId { get; set; }

    public SalaryCalculation? SalaryCalculation { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Draft";

    public int? ApprovedById { get; set; }

    public DateTime? ApprovalDate { get; set; }

    [MaxLength(500)]
    public string? ApprovalRemarks { get; set; }

    [MaxLength(500)]
    public string? Remarks { get; set; }

    [Required]
    public int SubscriptionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

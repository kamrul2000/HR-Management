using System.ComponentModel.DataAnnotations;

namespace HRM.Core.Entities;

public class SalaryCalculation
{
    public int Id { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    public Employee Employee { get; set; } = null!;

    [Required]
    public int SalaryStructureId { get; set; }

    public SalaryStructure SalaryStructure { get; set; } = null!;

    [Required]
    [Range(2000, 2100)]
    public int Year { get; set; }

    [Required]
    [Range(1, 12)]
    public int Month { get; set; }

    [Required]
    public int TotalWorkingDays { get; set; }

    [Required]
    public int PresentDays { get; set; }

    [Required]
    public int AbsentDays { get; set; }

    [Required]
    public int HalfDays { get; set; }

    [Required]
    public decimal UnpaidLeaveDays { get; set; }

    [Required]
    public decimal LateDeductionDays { get; set; }

    [Required]
    public int OvertimeMinutes { get; set; }

    [Required]
    public decimal BasicSalary { get; set; }

    [Required]
    public decimal GrossSalary { get; set; }

    [Required]
    public decimal TotalEarnings { get; set; }

    [Required]
    public decimal TotalDeductions { get; set; }

    [Required]
    public decimal OvertimePay { get; set; }

    public decimal BonusAmount { get; set; }

    public decimal LoanDeduction { get; set; }

    public decimal TaxDeduction { get; set; }

    [Required]
    public decimal NetSalary { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Draft";

    public string? CalculationDetails { get; set; }

    public DateTime? FinalizedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    [MaxLength(500)]
    public string? Remarks { get; set; }

    [Required]
    public int SubscriptionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<SalaryCalculationDetail> Details { get; set; }
        = new List<SalaryCalculationDetail>();
}

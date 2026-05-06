using System.ComponentModel.DataAnnotations;

namespace HRM.Core.Entities;

public class LoanInstallment
{
    public int Id { get; set; }

    [Required]
    public int EmployeeLoanId { get; set; }

    public EmployeeLoan EmployeeLoan { get; set; } = null!;

    [Required]
    public int EmployeeId { get; set; }

    public Employee Employee { get; set; } = null!;

    [Required]
    public int InstallmentNo { get; set; }

    [Required]
    [Range(1, 12)]
    public int DueMonth { get; set; }

    [Required]
    [Range(2000, 2100)]
    public int DueYear { get; set; }

    [Required]
    public decimal InstallmentAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public DateTime? PaidDate { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    public int? SalaryCalculationId { get; set; }

    public SalaryCalculation? SalaryCalculation { get; set; }

    [Required]
    public int SubscriptionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

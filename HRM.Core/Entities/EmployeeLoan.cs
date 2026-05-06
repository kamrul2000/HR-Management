using System.ComponentModel.DataAnnotations;

namespace HRM.Core.Entities;

public class EmployeeLoan
{
    public int Id { get; set; }

    [Required]
    public int LoanApplicationId { get; set; }

    public LoanApplication LoanApplication { get; set; } = null!;

    [Required]
    public int EmployeeId { get; set; }

    public Employee Employee { get; set; } = null!;

    [Required]
    [MaxLength(30)]
    public string LoanNo { get; set; } = string.Empty;

    [Required]
    public decimal PrincipalAmount { get; set; }

    [Required]
    public decimal InterestRate { get; set; }

    [MaxLength(20)]
    public string? InterestType { get; set; }

    [Required]
    public int TenureMonths { get; set; }

    [Required]
    public decimal MonthlyInstallment { get; set; }

    [Required]
    public decimal TotalRepayable { get; set; }

    [Required]
    public DateTime DisbursementDate { get; set; }

    [Required]
    [Range(1, 12)]
    public int FirstInstallmentMonth { get; set; }

    [Required]
    [Range(2000, 2100)]
    public int FirstInstallmentYear { get; set; }

    public decimal TotalPaid { get; set; }

    [Required]
    public decimal OutstandingBalance { get; set; }

    public int PaidInstallments { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Active";

    [MaxLength(500)]
    public string? Remarks { get; set; }

    [Required]
    public int SubscriptionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<LoanInstallment> Installments { get; set; }
        = new List<LoanInstallment>();
}

namespace HRM.Core.DTOs.Bonus;

public class BonusResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeFullName { get; set; } = string.Empty;
    public string BonusType { get; set; } = string.Empty;
    public string BonusTypeLabel { get; set; } = string.Empty;
    public string BonusTitle { get; set; } = string.Empty;
    public string CalculationBasis { get; set; } = string.Empty;
    public string CalculationBasisLabel { get; set; } = string.Empty;
    public decimal? BasisPercentage { get; set; }
    public decimal BasicSalarySnapshot { get; set; }
    public decimal GrossSalarySnapshot { get; set; }
    public decimal ComputedAmount { get; set; }
    public string ComputedAmountFormatted { get; set; } = string.Empty;
    public decimal FinalAmount { get; set; }
    public string FinalAmountFormatted { get; set; } = string.Empty;
    public int DisbursementMonth { get; set; }
    public int DisbursementYear { get; set; }
    public string DisbursementPeriodLabel { get; set; } = string.Empty;
    public bool IsDisbursedWithSalary { get; set; }
    public int? SalaryCalculationId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public int? ApprovedById { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public string? ApprovalDateFormatted { get; set; }
    public string? ApprovalRemarks { get; set; }
    public string? Remarks { get; set; }
    public int SubscriptionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

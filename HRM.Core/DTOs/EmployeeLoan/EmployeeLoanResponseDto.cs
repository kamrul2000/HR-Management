namespace HRM.Core.DTOs.EmployeeLoan;

public class EmployeeLoanResponseDto
{
    public int Id { get; set; }
    public string LoanNo { get; set; } = string.Empty;
    public int LoanApplicationId { get; set; }
    public string ApplicationNo { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeFullName { get; set; } = string.Empty;
    public string LoanType { get; set; } = string.Empty;
    public decimal PrincipalAmount { get; set; }
    public string PrincipalAmountFormatted { get; set; } = string.Empty;
    public decimal InterestRate { get; set; }
    public string? InterestType { get; set; }
    public string InterestTypeLabel { get; set; } = string.Empty;
    public int TenureMonths { get; set; }
    public string TenureLabel { get; set; } = string.Empty;
    public decimal MonthlyInstallment { get; set; }
    public string MonthlyInstallmentFormatted { get; set; } = string.Empty;
    public decimal TotalRepayable { get; set; }
    public string TotalRepayableFormatted { get; set; } = string.Empty;
    public DateTime DisbursementDate { get; set; }
    public string DisbursementDateFormatted { get; set; } = string.Empty;
    public int FirstInstallmentMonth { get; set; }
    public int FirstInstallmentYear { get; set; }
    public string FirstInstallmentPeriodLabel { get; set; } = string.Empty;
    public decimal TotalPaid { get; set; }
    public string TotalPaidFormatted { get; set; } = string.Empty;
    public decimal OutstandingBalance { get; set; }
    public string OutstandingBalanceFormatted { get; set; } = string.Empty;
    public int PaidInstallments { get; set; }
    public int RemainingInstallments { get; set; }
    public decimal CompletionPercentage { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public List<LoanInstallmentSummaryDto> Installments { get; set; } = new();
    public int SubscriptionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

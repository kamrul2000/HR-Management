namespace HRM.Core.DTOs.LoanApproval;

public class LoanApprovalResponseDto
{
    public int Id { get; set; }
    public int LoanApplicationId { get; set; }
    public string ApplicationNo { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeFullName { get; set; } = string.Empty;
    public decimal RequestedAmount { get; set; }
    public decimal? RecommendedAmount { get; set; }
    public int ApprovedById { get; set; }
    public string Decision { get; set; } = string.Empty;
    public string DecisionLabel { get; set; } = string.Empty;
    public decimal? ApprovedAmount { get; set; }
    public string? ApprovedAmountFormatted { get; set; }
    public int? ApprovedTenureMonths { get; set; }
    public string? TenureLabel { get; set; }
    public decimal? InterestRate { get; set; }
    public string? InterestType { get; set; }
    public string? InterestTypeLabel { get; set; }
    public decimal? MonthlyInstallment { get; set; }
    public string? MonthlyInstallmentFormatted { get; set; }
    public decimal? TotalRepayable { get; set; }
    public string? TotalRepayableFormatted { get; set; }
    public string Remarks { get; set; } = string.Empty;
    public int SubscriptionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

namespace HRM.Core.DTOs.PfInterest;

public class EmployeePfInterestResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeFullName { get; set; } = string.Empty;
    public int PfInterestRateId { get; set; }
    public string FiscalYear { get; set; } = string.Empty;
    public decimal InterestRate { get; set; }
    public string InterestRateLabel { get; set; } = string.Empty;
    public decimal OpeningBalance { get; set; }
    public string OpeningBalanceFormatted { get; set; } = string.Empty;
    public decimal TotalContributionsForYear { get; set; }
    public string TotalContributionsFormatted { get; set; } = string.Empty;
    public decimal AverageBalance { get; set; }
    public string AverageBalanceFormatted { get; set; } = string.Empty;
    public decimal InterestAmount { get; set; }
    public string InterestAmountFormatted { get; set; } = string.Empty;
    public decimal ClosingBalance { get; set; }
    public string ClosingBalanceFormatted { get; set; } = string.Empty;
    public int SubscriptionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

namespace HRM.Core.DTOs.PfInterest;

public class PfInterestReportDto
{
    public string FiscalYear { get; set; } = string.Empty;
    public decimal InterestRate { get; set; }
    public string InterestRateLabel { get; set; } = string.Empty;
    public int TotalEmployees { get; set; }
    public decimal TotalOpeningBalance { get; set; }
    public string TotalOpeningBalanceFormatted { get; set; } = string.Empty;
    public decimal TotalContributions { get; set; }
    public string TotalContributionsFormatted { get; set; } = string.Empty;
    public decimal TotalInterestCredited { get; set; }
    public string TotalInterestCreditedFormatted { get; set; } = string.Empty;
    public decimal TotalClosingBalance { get; set; }
    public string TotalClosingBalanceFormatted { get; set; } = string.Empty;
    public List<EmployeePfInterestResponseDto> Details { get; set; } = new();
}

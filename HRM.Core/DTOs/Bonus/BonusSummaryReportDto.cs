namespace HRM.Core.DTOs.Bonus;

public class BonusSummaryReportDto
{
    public int DisbursementYear { get; set; }
    public int DisbursementMonth { get; set; }
    public string MonthLabel { get; set; } = string.Empty;
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public int TotalEmployees { get; set; }
    public int ApprovedCount { get; set; }
    public int DisbursedCount { get; set; }
    public decimal TotalComputedAmount { get; set; }
    public decimal TotalFinalAmount { get; set; }
    public string TotalFinalAmountFormatted { get; set; } = string.Empty;
    public Dictionary<string, decimal> AmountByBonusType { get; set; } = new();
}

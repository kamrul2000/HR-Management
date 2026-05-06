namespace HRM.Core.DTOs.LoanRecommendation;

public class LoanRecommendationResponseDto
{
    public int Id { get; set; }
    public int LoanApplicationId { get; set; }
    public string ApplicationNo { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeFullName { get; set; } = string.Empty;
    public decimal RequestedAmount { get; set; }
    public string RequestedAmountFormatted { get; set; } = string.Empty;
    public int RequestedTenureMonths { get; set; }
    public int RecommendedById { get; set; }
    public string Decision { get; set; } = string.Empty;
    public string DecisionLabel { get; set; } = string.Empty;
    public decimal? RecommendedAmount { get; set; }
    public string? RecommendedAmountFormatted { get; set; }
    public int? RecommendedTenureMonths { get; set; }
    public decimal? AmountDifference { get; set; }
    public string Remarks { get; set; } = string.Empty;
    public int SubscriptionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

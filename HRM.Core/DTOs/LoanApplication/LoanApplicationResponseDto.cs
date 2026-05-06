namespace HRM.Core.DTOs.LoanApplication;

public class LoanApplicationResponseDto
{
    public int Id { get; set; }
    public string ApplicationNo { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeFullName { get; set; } = string.Empty;
    public string LoanType { get; set; } = string.Empty;
    public string LoanTypeLabel { get; set; } = string.Empty;
    public decimal RequestedAmount { get; set; }
    public string RequestedAmountFormatted { get; set; } = string.Empty;
    public int RequestedTenureMonths { get; set; }
    public string TenureLabel { get; set; } = string.Empty;
    public decimal EstimatedMonthlyInstallment { get; set; }
    public string EstimatedMonthlyInstallmentFormatted { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public int? RecommendedById { get; set; }
    public DateTime? RecommendationDate { get; set; }
    public string? RecommendationDateFormatted { get; set; }
    public string? RecommendationRemarks { get; set; }
    public int? RejectedById { get; set; }
    public DateTime? RejectionDate { get; set; }
    public string? RejectionDateFormatted { get; set; }
    public string? RejectionRemarks { get; set; }
    public int SubscriptionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

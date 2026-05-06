namespace HRM.Core.DTOs.GratuityCalculation;

public class GratuityCalculationResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeFullName { get; set; } = string.Empty;
    public int GratuityRuleId { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public DateTime SeparationDate { get; set; }
    public string SeparationDateFormatted { get; set; } = string.Empty;
    public DateTime JoiningDate { get; set; }
    public string JoiningDateFormatted { get; set; } = string.Empty;
    public int TotalServiceDays { get; set; }
    public decimal TotalServiceYears { get; set; }
    public string ServicePeriodLabel { get; set; } = string.Empty;
    public decimal EligibleYears { get; set; }
    public string CalculationBasis { get; set; } = string.Empty;
    public string CalculationBasisLabel { get; set; } = string.Empty;
    public decimal MonthlySalaryUsed { get; set; }
    public string MonthlySalaryFormatted { get; set; } = string.Empty;
    public decimal DailySalary { get; set; }
    public string DailySalaryFormatted { get; set; } = string.Empty;
    public decimal RatePerYear { get; set; }
    public decimal GratuityBeforeCap { get; set; }
    public string GratuityBeforeCapFormatted { get; set; } = string.Empty;
    public decimal GratuityAmount { get; set; }
    public string GratuityAmountFormatted { get; set; } = string.Empty;
    public bool IsCapApplied { get; set; }
    public bool IsEligible { get; set; }
    public string? IneligibilityReason { get; set; }
    public int? SeparationId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public int SubscriptionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

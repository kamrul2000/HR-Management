namespace HRM.Core.DTOs.GratuitySetup;

public class GratuityPreviewResultDto
{
    public int EmployeeId { get; set; }
    public string EmployeeFullName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public DateTime JoiningDate { get; set; }
    public string JoiningDateFormatted { get; set; } = string.Empty;
    public DateTime SeparationDate { get; set; }
    public string SeparationDateFormatted { get; set; } = string.Empty;
    public decimal TotalServiceYears { get; set; }
    public string ServicePeriodLabel { get; set; } = string.Empty;
    public bool IsEligible { get; set; }
    public string IneligibilityReason { get; set; } = string.Empty;
    public decimal MonthlySalaryUsed { get; set; }
    public string MonthlySalaryFormatted { get; set; } = string.Empty;
    public decimal DailySalary { get; set; }
    public decimal EligibleYears { get; set; }
    public decimal RatePerYear { get; set; }
    public decimal GratuityBeforeCap { get; set; }
    public decimal GratuityAmount { get; set; }
    public string GratuityAmountFormatted { get; set; } = string.Empty;
    public bool IsCapApplied { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public string CalculationBasis { get; set; } = string.Empty;
}

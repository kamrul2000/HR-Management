namespace HRM.Core.DTOs.PfContribution;

public class EmployeePfContributionResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeFullName { get; set; } = string.Empty;
    public int PfRuleId { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public string PeriodLabel { get; set; } = string.Empty;
    public decimal PfBase { get; set; }
    public string PfBaseFormatted { get; set; } = string.Empty;
    public decimal EmployeeContributionRate { get; set; }
    public decimal EmployerContributionRate { get; set; }
    public decimal EmployeeContribution { get; set; }
    public string EmployeeContributionFormatted { get; set; } = string.Empty;
    public decimal EmployerContribution { get; set; }
    public string EmployerContributionFormatted { get; set; } = string.Empty;
    public decimal TotalContribution { get; set; }
    public string TotalContributionFormatted { get; set; } = string.Empty;
    public int? SalaryCalculationId { get; set; }
    public int SubscriptionId { get; set; }
    public DateTime CreatedAt { get; set; }
}

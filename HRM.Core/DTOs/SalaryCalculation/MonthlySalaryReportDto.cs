namespace HRM.Core.DTOs.SalaryCalculation;

public class MonthlySalaryReportDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthLabel { get; set; } = string.Empty;
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public int TotalEmployees { get; set; }
    public int FinalizedCount { get; set; }
    public int DraftCount { get; set; }
    public decimal TotalGrossSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalOvertimePay { get; set; }
    public decimal TotalNetSalary { get; set; }
    public string TotalNetSalaryFormatted { get; set; } = string.Empty;
    public List<SalaryCalculationResponseDto> Calculations { get; set; } = new();
}

namespace HRM.Core.DTOs.PfContribution;

public class PfMonthlyReportDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthLabel { get; set; } = string.Empty;
    public int TotalEmployees { get; set; }
    public decimal TotalEmployeeContribution { get; set; }
    public string TotalEmployeeContributionFormatted { get; set; } = string.Empty;
    public decimal TotalEmployerContribution { get; set; }
    public string TotalEmployerContributionFormatted { get; set; } = string.Empty;
    public decimal TotalContribution { get; set; }
    public string TotalContributionFormatted { get; set; } = string.Empty;
    public List<EmployeePfContributionResponseDto> Contributions { get; set; } = new();
}

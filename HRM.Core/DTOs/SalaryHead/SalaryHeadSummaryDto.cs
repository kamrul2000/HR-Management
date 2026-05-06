namespace HRM.Core.DTOs.SalaryHead;

public class SalaryHeadSummaryDto
{
    public int Id { get; set; }
    public string HeadName { get; set; } = string.Empty;
    public string HeadCode { get; set; } = string.Empty;
    public string HeadType { get; set; } = string.Empty;
    public string CalculationMethod { get; set; } = string.Empty;
    public decimal? Percentage { get; set; }
    public bool IsFixed { get; set; }
    public int DisplayOrder { get; set; }
}

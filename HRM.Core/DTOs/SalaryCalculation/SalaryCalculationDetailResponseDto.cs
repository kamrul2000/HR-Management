namespace HRM.Core.DTOs.SalaryCalculation;

public class SalaryCalculationDetailResponseDto
{
    public int Id { get; set; }
    public int SalaryHeadId { get; set; }
    public string HeadName { get; set; } = string.Empty;
    public string HeadCode { get; set; } = string.Empty;
    public string HeadType { get; set; } = string.Empty;
    public string HeadTypeLabel { get; set; } = string.Empty;
    public string CalculationMethod { get; set; } = string.Empty;
    public decimal? BaseAmount { get; set; }
    public decimal? AppliedPercentage { get; set; }
    public decimal ComputedAmount { get; set; }
    public string ComputedAmountFormatted { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

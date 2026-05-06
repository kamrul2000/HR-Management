namespace HRM.Core.DTOs.GratuityCalculation;

public class GratuityReportDto
{
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public int TotalRecords { get; set; }
    public int EligibleCount { get; set; }
    public int IneligibleCount { get; set; }
    public decimal TotalGratuityAmount { get; set; }
    public string TotalGratuityAmountFormatted { get; set; } = string.Empty;
    public List<GratuityCalculationResponseDto> Details { get; set; } = new();
}

namespace HRM.Core.DTOs.SalaryHead;

public class SalaryHeadResponseDto
{
    public int Id { get; set; }
    public string HeadName { get; set; } = string.Empty;
    public string HeadCode { get; set; } = string.Empty;
    public string HeadType { get; set; } = string.Empty;
    public string HeadTypeLabel { get; set; } = string.Empty;
    public string CalculationMethod { get; set; } = string.Empty;
    public string CalculationMethodLabel { get; set; } = string.Empty;
    public string? CalculationWarning { get; set; }
    public decimal? Percentage { get; set; }
    public int? BaseHeadId { get; set; }
    public string? BaseHeadName { get; set; }
    public bool IsFixed { get; set; }
    public bool IsTaxable { get; set; }
    public bool IsProvidentFundApplicable { get; set; }
    public int DisplayOrder { get; set; }
    public string? Description { get; set; }
    public int SubscriptionId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

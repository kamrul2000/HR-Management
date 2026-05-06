namespace HRM.Core.DTOs.SalaryCreate;

public class SalaryStructureItemResponseDto
{
    public int Id { get; set; }
    public int SalaryHeadId { get; set; }
    public string HeadName { get; set; } = string.Empty;
    public string HeadCode { get; set; } = string.Empty;
    public string HeadType { get; set; } = string.Empty;
    public string HeadTypeLabel { get; set; } = string.Empty;
    public string CalculationMethod { get; set; } = string.Empty;
    public decimal? FixedAmount { get; set; }
    public decimal? OverridePercentage { get; set; }
    public decimal? EffectivePercentage { get; set; }
    public bool IsTaxable { get; set; }
    public bool IsProvidentFundApplicable { get; set; }
    public int DisplayOrder { get; set; }
}

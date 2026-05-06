namespace HRM.Core.DTOs.SalaryCreate;

public class SalaryStructureHistoryDto
{
    public int Id { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public string EffectiveFromFormatted { get; set; } = string.Empty;
    public DateTime? EffectiveTo { get; set; }
    public string? EffectiveToFormatted { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal EstimatedGrossSalary { get; set; }
    public bool IsActive { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; }
}

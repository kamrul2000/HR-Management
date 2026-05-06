namespace HRM.Core.DTOs.SalaryCreate;

public class SalaryStructureResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeFullName { get; set; } = string.Empty;
    public DateTime EffectiveFrom { get; set; }
    public string EffectiveFromFormatted { get; set; } = string.Empty;
    public DateTime? EffectiveTo { get; set; }
    public string? EffectiveToFormatted { get; set; }
    public decimal BasicSalary { get; set; }
    public string BasicSalaryFormatted { get; set; } = string.Empty;
    public decimal EstimatedGrossSalary { get; set; }
    public decimal EstimatedNetSalary { get; set; }
    public decimal EstimatedDeductions { get; set; }
    public bool IsActive { get; set; }
    public string? Remarks { get; set; }
    public List<SalaryStructureItemResponseDto> Items { get; set; } = new();
    public int SubscriptionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

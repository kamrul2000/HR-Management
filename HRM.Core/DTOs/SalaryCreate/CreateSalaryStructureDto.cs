using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.SalaryCreate;

public class CreateSalaryStructureDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int EmployeeId { get; set; }

    [Required]
    public DateTime EffectiveFrom { get; set; }

    [Required]
    [Range(typeof(decimal), "0.01", "9999999.99")]
    public decimal BasicSalary { get; set; }

    [MaxLength(500)]
    public string? Remarks { get; set; }

    [Required]
    [MinLength(1)]
    public List<SalaryStructureItemDto> Items { get; set; } = new();
}

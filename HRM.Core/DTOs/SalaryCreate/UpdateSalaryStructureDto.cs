using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.SalaryCreate;

public class UpdateSalaryStructureDto
{
    [MaxLength(500)]
    public string? Remarks { get; set; }

    [Required]
    [MinLength(1)]
    public List<SalaryStructureItemDto> Items { get; set; } = new();
}

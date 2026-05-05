using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.Department;

public class UpdateDepartmentDto
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int BranchId { get; set; }

    [Required]
    public bool IsActive { get; set; }
}

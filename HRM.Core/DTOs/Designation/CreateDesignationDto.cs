using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.Designation;

public class CreateDesignationDto
{
    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string? Grade { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int DepartmentId { get; set; }
}

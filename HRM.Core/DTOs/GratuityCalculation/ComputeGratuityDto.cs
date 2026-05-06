using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.GratuityCalculation;

public class ComputeGratuityDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int EmployeeId { get; set; }

    [Required]
    public DateTime SeparationDate { get; set; }

    public int? GratuityRuleId { get; set; }

    [MaxLength(500)]
    public string? Remarks { get; set; }
}

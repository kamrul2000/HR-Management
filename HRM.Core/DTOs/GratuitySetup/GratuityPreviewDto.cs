using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.GratuitySetup;

public class GratuityPreviewDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int EmployeeId { get; set; }

    [Required]
    public DateTime SeparationDate { get; set; }

    public int? GratuityRuleId { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.EmployeeSeparation;

public class CancelSeparationDto
{
    [Required]
    [MaxLength(500)]
    public string CancellationReason { get; set; } = string.Empty;
}

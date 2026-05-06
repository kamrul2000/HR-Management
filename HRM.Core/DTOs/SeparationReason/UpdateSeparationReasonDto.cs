using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.SeparationReason;

public class UpdateSeparationReasonDto
{
    [Required]
    [MaxLength(150)]
    public string ReasonName { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string SeparationType { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [Range(1, 999)]
    public int DisplayOrder { get; set; }

    [Required]
    public bool IsActive { get; set; }
}

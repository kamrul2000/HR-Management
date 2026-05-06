using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.AdditionalInfo;

public class UpdateExperienceDto
{
    [Required]
    [MaxLength(200)]
    public string OrganizationName { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Designation { get; set; } = string.Empty;

    [Required]
    public DateTime FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public bool IsCurrent { get; set; }

    [MaxLength(1000)]
    public string? Responsibilities { get; set; }

    [MaxLength(500)]
    public string? ReasonForLeaving { get; set; }
}

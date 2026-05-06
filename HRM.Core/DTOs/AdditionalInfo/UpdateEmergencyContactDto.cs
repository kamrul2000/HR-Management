using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.AdditionalInfo;

public class UpdateEmergencyContactDto
{
    [Required]
    [MaxLength(150)]
    public string ContactName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Relationship { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? AlternatePhone { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    public bool IsPrimary { get; set; }
}

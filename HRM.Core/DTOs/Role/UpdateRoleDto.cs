using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.Role;

public class UpdateRoleDto
{
    [Required]
    [MaxLength(100)]
    public string RoleName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public bool IsActive { get; set; }
}

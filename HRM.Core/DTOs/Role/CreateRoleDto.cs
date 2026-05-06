using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.Role;

public class CreateRoleDto
{
    [Required]
    [MaxLength(100)]
    public string RoleName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }
}

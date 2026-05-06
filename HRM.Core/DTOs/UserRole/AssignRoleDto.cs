using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.UserRole;

public class AssignRoleDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int UserId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int RoleId { get; set; }
}

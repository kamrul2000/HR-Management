using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.Branch;

public class UpdateBranchDto
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ManagerName { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int CompanyId { get; set; }

    [Required]
    public bool IsActive { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.Permission;

public class UpsertPermissionDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int RoleId { get; set; }

    [Required]
    [MaxLength(50)]
    public string ModuleCode { get; set; } = string.Empty;

    public bool CanView { get; set; }
    public bool CanCreate { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanApprove { get; set; }
    public bool CanExport { get; set; }
}

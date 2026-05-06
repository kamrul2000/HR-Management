using System.ComponentModel.DataAnnotations;

namespace HRM.Core.Entities;

public class Permission
{
    public int Id { get; set; }

    [Required]
    public int RoleId { get; set; }

    public Role Role { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string ModuleCode { get; set; } = string.Empty;

    public bool CanView { get; set; }

    public bool CanCreate { get; set; }

    public bool CanEdit { get; set; }

    public bool CanDelete { get; set; }

    public bool CanApprove { get; set; }

    public bool CanExport { get; set; }

    [Required]
    public int SubscriptionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

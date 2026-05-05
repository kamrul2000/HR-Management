using System.ComponentModel.DataAnnotations;

namespace HRM.Core.Entities;

// Stub introduced in Module 3 so Branch.Departments navigation compiles.
// Module 4 extends this with full department-management fields.
public class Department
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    public int BranchId { get; set; }

    public Branch Branch { get; set; } = null!;

    [Required]
    public int SubscriptionId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

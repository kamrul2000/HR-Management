using System.ComponentModel.DataAnnotations;

namespace HRM.Core.Entities;

public class Department
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public int BranchId { get; set; }

    public Branch Branch { get; set; } = null!;

    [Required]
    public int SubscriptionId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<Designation> Designations { get; set; } = new List<Designation>();
}

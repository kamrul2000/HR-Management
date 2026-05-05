using System.ComponentModel.DataAnnotations;

namespace HRM.Core.Entities;

public class Designation
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string? Grade { get; set; }

    [Required]
    public int DepartmentId { get; set; }

    public Department Department { get; set; } = null!;

    [Required]
    public int SubscriptionId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}

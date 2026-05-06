using System.ComponentModel.DataAnnotations;

namespace HRM.Core.Entities;

public class SalaryStructure
{
    public int Id { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    public Employee Employee { get; set; } = null!;

    [Required]
    public DateTime EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    [Required]
    public decimal BasicSalary { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string? Remarks { get; set; }

    [Required]
    public int SubscriptionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<SalaryStructureItem> Items { get; set; } = new List<SalaryStructureItem>();
}

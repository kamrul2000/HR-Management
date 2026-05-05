using System.ComponentModel.DataAnnotations;

namespace HRM.Core.Entities;

public class OffDay
{
    public int Id { get; set; }

    [Required]
    [Range(0, 6)]
    public int DayOfWeek { get; set; }

    [Required]
    [MaxLength(20)]
    public string DayName { get; set; } = string.Empty;

    public int? BranchId { get; set; }

    public Branch? Branch { get; set; }

    [Required]
    public int SubscriptionId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

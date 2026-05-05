using System.ComponentModel.DataAnnotations;

namespace HRM.Core.Entities;

public class DutySlot
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string SlotName { get; set; } = string.Empty;

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    [Required]
    [Range(0, 180)]
    public int BreakDurationMinutes { get; set; }

    [Required]
    [Range(0, 120)]
    public int LateToleranceMinutes { get; set; }

    [Required]
    public decimal TotalWorkingHours { get; set; }

    public bool IsNightShift { get; set; }

    [Required]
    public int SubscriptionId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

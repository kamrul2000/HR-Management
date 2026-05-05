using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.DutySlot;

public class UpdateDutySlotDto
{
    [Required]
    [MaxLength(100)]
    public string SlotName { get; set; } = string.Empty;

    /// <summary>Shift start time. Format: "HH:mm:ss" e.g. "08:00:00".</summary>
    [Required]
    public TimeSpan StartTime { get; set; }

    /// <summary>Shift end time. Format: "HH:mm:ss" e.g. "17:00:00". For night shifts, EndTime will be less than StartTime.</summary>
    [Required]
    public TimeSpan EndTime { get; set; }

    [Required]
    [Range(0, 180)]
    public int BreakDurationMinutes { get; set; }

    [Required]
    [Range(0, 120)]
    public int LateToleranceMinutes { get; set; }

    [Required]
    public bool IsActive { get; set; }
}

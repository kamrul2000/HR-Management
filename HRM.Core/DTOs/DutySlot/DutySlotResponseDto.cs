namespace HRM.Core.DTOs.DutySlot;

public class DutySlotResponseDto
{
    public int Id { get; set; }
    public string SlotName { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string StartTimeFormatted { get; set; } = string.Empty;
    public string EndTimeFormatted { get; set; } = string.Empty;
    public int BreakDurationMinutes { get; set; }
    public int LateToleranceMinutes { get; set; }
    public decimal TotalWorkingHours { get; set; }
    public string TotalWorkingHoursFormatted { get; set; } = string.Empty;
    public bool IsNightShift { get; set; }
    public int SubscriptionId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

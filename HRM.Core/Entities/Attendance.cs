using System.ComponentModel.DataAnnotations;

namespace HRM.Core.Entities;

public class Attendance
{
    public int Id { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    public Employee Employee { get; set; } = null!;

    [Required]
    public int DutySlotId { get; set; }

    public DutySlot DutySlot { get; set; } = null!;

    [Required]
    public DateTime AttendanceDate { get; set; }

    public TimeSpan? PunchInTime { get; set; }

    public TimeSpan? PunchOutTime { get; set; }

    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = string.Empty;

    public bool IsLate { get; set; }

    public int LateMinutes { get; set; }

    public int ActualWorkingMinutes { get; set; }

    [Required]
    public int ScheduledWorkingMinutes { get; set; }

    public int OvertimeMinutes { get; set; }

    [MaxLength(500)]
    public string? Remarks { get; set; }

    [Required]
    public int SubscriptionId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

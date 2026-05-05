using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.Attendance;

public class CreateAttendanceDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int EmployeeId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int DutySlotId { get; set; }

    [Required]
    public DateTime AttendanceDate { get; set; }

    /// <summary>Actual punch-in time. Format: "HH:mm:ss". Null if absent.</summary>
    public TimeSpan? PunchInTime { get; set; }

    /// <summary>Actual punch-out time. Format: "HH:mm:ss". Null if punch-out pending.</summary>
    public TimeSpan? PunchOutTime { get; set; }

    [MaxLength(500)]
    public string? Remarks { get; set; }
}

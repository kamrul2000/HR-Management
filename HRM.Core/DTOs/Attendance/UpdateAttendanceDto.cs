using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.Attendance;

public class UpdateAttendanceDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int DutySlotId { get; set; }

    public TimeSpan? PunchInTime { get; set; }

    public TimeSpan? PunchOutTime { get; set; }

    [MaxLength(500)]
    public string? Remarks { get; set; }
}

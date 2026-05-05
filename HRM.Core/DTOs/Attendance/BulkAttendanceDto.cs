using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.Attendance;

public class BulkAttendanceDto
{
    [Required]
    public DateTime AttendanceDate { get; set; }

    [Required]
    [MinLength(1)]
    public List<EmployeeAttendanceEntryDto> Entries { get; set; } = new();
}

public class EmployeeAttendanceEntryDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int EmployeeId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int DutySlotId { get; set; }

    public TimeSpan? PunchInTime { get; set; }
    public TimeSpan? PunchOutTime { get; set; }

    [MaxLength(500)]
    public string? Remarks { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.Overtime;

public class CreateOvertimeDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int EmployeeId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int AttendanceId { get; set; }

    [Required]
    [Range(1, 720)]
    public int RequestedMinutes { get; set; }

    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}

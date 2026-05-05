using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.LeaveApplication;

public class CreateLeaveApplicationDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int EmployeeId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int LeaveTypeId { get; set; }

    [Required]
    public DateTime FromDate { get; set; }

    [Required]
    public DateTime ToDate { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Reason { get; set; } = string.Empty;
}

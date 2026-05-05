using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.LeaveAllotment;

public class BulkCreateLeaveAllotmentDto
{
    [Required]
    public List<int> EmployeeIds { get; set; } = new();

    [Required]
    [Range(1, int.MaxValue)]
    public int LeaveTypeId { get; set; }

    [Required]
    [Range(2000, 2100)]
    public int Year { get; set; }

    [Required]
    [Range(typeof(decimal), "0.5", "365")]
    public decimal AllocatedDays { get; set; }
}

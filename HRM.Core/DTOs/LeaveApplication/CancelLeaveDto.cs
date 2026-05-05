using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.LeaveApplication;

public class CancelLeaveDto
{
    [Required]
    [MaxLength(500)]
    public string CancellationReason { get; set; } = string.Empty;
}

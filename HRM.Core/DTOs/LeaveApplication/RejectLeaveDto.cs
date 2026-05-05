using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.LeaveApplication;

public class RejectLeaveDto
{
    [Required]
    [MaxLength(500)]
    public string ApprovalRemarks { get; set; } = string.Empty;
}

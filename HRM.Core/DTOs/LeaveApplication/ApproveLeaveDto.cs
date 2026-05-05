using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.LeaveApplication;

public class ApproveLeaveDto
{
    [MaxLength(500)]
    public string? ApprovalRemarks { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.Overtime;

public class ApproveOvertimeDto
{
    /// <summary>
    /// Approved overtime in minutes. If omitted, defaults to RequestedMinutes.
    /// Approver may approve less than requested but never more.
    /// </summary>
    [Range(1, 720)]
    public int? ApprovedMinutes { get; set; }

    [MaxLength(500)]
    public string? ApprovalRemarks { get; set; }
}

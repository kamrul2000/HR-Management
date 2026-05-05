using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.Overtime;

public class RejectOvertimeDto
{
    [Required]
    [MaxLength(500)]
    public string ApprovalRemarks { get; set; } = string.Empty;
}

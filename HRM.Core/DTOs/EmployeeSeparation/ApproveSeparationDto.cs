using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.EmployeeSeparation;

public class ApproveSeparationDto
{
    [MaxLength(500)]
    public string? ApprovalRemarks { get; set; }
}

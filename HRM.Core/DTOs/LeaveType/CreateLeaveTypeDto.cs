using System.ComponentModel.DataAnnotations;

namespace HRM.Core.DTOs.LeaveType;

public class CreateLeaveTypeDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public bool IsPaid { get; set; }

    [Required]
    public bool IsCarryForward { get; set; }

    [Required]
    [Range(0, 365)]
    public int MaxCarryForwardDays { get; set; }

    [Required]
    public bool RequiresApproval { get; set; }

    [Required]
    public bool RequiresDocument { get; set; }

    [Required]
    [Range(0, 90)]
    public int MinNoticeDays { get; set; }

    [Range(1, 365)]
    public int? MaxConsecutiveDays { get; set; }

    [MaxLength(20)]
    public string? GenderRestriction { get; set; }
}

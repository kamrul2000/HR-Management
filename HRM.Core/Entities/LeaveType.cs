using System.ComponentModel.DataAnnotations;

namespace HRM.Core.Entities;

public class LeaveType
{
    public int Id { get; set; }

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

    public bool IsCarryForward { get; set; }

    [Range(0, 365)]
    public int MaxCarryForwardDays { get; set; }

    public bool RequiresApproval { get; set; } = true;

    public bool RequiresDocument { get; set; }

    [Range(0, 90)]
    public int MinNoticeDays { get; set; }

    [Range(1, 365)]
    public int? MaxConsecutiveDays { get; set; }

    [MaxLength(20)]
    public string? GenderRestriction { get; set; }

    [Required]
    public int SubscriptionId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<LeaveAllotment> LeaveAllotments { get; set; } = new List<LeaveAllotment>();
    public ICollection<LeaveApplication> LeaveApplications { get; set; } = new List<LeaveApplication>();
}
